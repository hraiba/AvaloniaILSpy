#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.Util;
using ICSharpCode.ILSpy.Controls.FileLoaders;

namespace ICSharpCode.ILSpy;

class AssemblyListSnapshot
{
    readonly ImmutableArray<LoadedAssembly> assemblies;
    Dictionary<string, MetadataFile>? asmLookupByFullName;
    Dictionary<string, MetadataFile>? asmLookupByShortName;
    Dictionary<string, List<(MetadataFile module, Version version)>>? asmLookupByShortNameGrouped;
    public ImmutableArray<LoadedAssembly> Assemblies => assemblies;

    public AssemblyListSnapshot(ImmutableArray<LoadedAssembly> assemblies)
    {
        this.assemblies = assemblies;
    }

    public async Task<MetadataFile?> TryGetModuleAsync(IAssemblyReference reference, string tfm)
    {
        bool isWinRT = reference.IsWindowsRuntime;
        if (tfm.StartsWith(".NETFramework,Version=v4.", StringComparison.Ordinal))
        {
            tfm = ".NETFramework,Version=v4";
        }
        string key = tfm + ";" + (isWinRT ? reference.Name : reference.FullName);
        var lookup = LazyInit.VolatileRead(ref isWinRT ? ref asmLookupByShortName : ref asmLookupByFullName);
        if (lookup == null)
        {
            lookup = await CreateLoadedAssemblyLookupAsync(shortNames: isWinRT).ConfigureAwait(false);
            lookup = LazyInit.GetOrSet(ref isWinRT ? ref asmLookupByShortName : ref asmLookupByFullName, lookup);
        }
        if (lookup.TryGetValue(key, out MetadataFile? module))
        {
            return module;
        }

        return null;
    }

    public async Task<MetadataFile?> TryGetSimilarModuleAsync(IAssemblyReference reference)
    {
        var lookup = LazyInit.VolatileRead(ref asmLookupByShortNameGrouped);
        if (lookup == null)
        {
            lookup = await CreateLoadedAssemblyShortNameGroupLookupAsync().ConfigureAwait(false);
            lookup = LazyInit.GetOrSet(ref asmLookupByShortNameGrouped, lookup);
        }

        if (!lookup.TryGetValue(reference.Name, out var candidates))
        {
            return null;
        }

        return candidates.FirstOrDefault(c => c.version >= reference.Version).module ?? candidates.Last().module;
    }

    private async Task<Dictionary<string, MetadataFile>> CreateLoadedAssemblyLookupAsync(bool shortNames)
    {
        var result = new Dictionary<string, MetadataFile>(StringComparer.OrdinalIgnoreCase);
        foreach (LoadedAssembly loaded in assemblies)
        {
            try
            {
                var module = await loaded.GetMetadataFileOrNullAsync().ConfigureAwait(false);
                if (module == null)
                {
                    continue;
                }

                var reader = module.Metadata;
                if (reader?.IsAssembly != true)
                {
                    continue;
                }

                string tfm = await loaded.GetTargetFrameworkIdAsync().ConfigureAwait(false);
                if (tfm.StartsWith(".NETFramework,Version=v4.", StringComparison.Ordinal))
                {
                    tfm = ".NETFramework,Version=v4";
                }
                string key = tfm + ";"
                    + (shortNames ? module.Name : module.FullName);
                if (!result.ContainsKey(key))
                {
                    result.Add(key, module);
                }
            }
            catch (BadImageFormatException)
            {
                continue;
            }
        }
        return result;
    }

    private async Task<Dictionary<string, List<(MetadataFile module, Version version)>>> CreateLoadedAssemblyShortNameGroupLookupAsync()
    {
        var result = new Dictionary<string, List<(MetadataFile module, Version version)>>(StringComparer.OrdinalIgnoreCase);

        foreach (LoadedAssembly loaded in assemblies)
        {
            try
            {
                var module = await loaded.GetMetadataFileOrNullAsync().ConfigureAwait(false);
                var reader = module?.Metadata;
                if (reader?.IsAssembly != true)
                {
                    continue;
                }

                var asmDef = reader.GetAssemblyDefinition();
                var asmDefName = reader.GetString(asmDef.Name);

                var line = (module!, version: asmDef.Version);

                if (!result.TryGetValue(asmDefName, out var existing))
                {
                    existing = [];
                    result.Add(asmDefName, existing);
                    existing.Add(line);
                    continue;
                }

                int index = existing.BinarySearch(line.version, l => l.version);
                index = index < 0 ? ~index : index + 1;
                existing.Insert(index, line);
            }
            catch (BadImageFormatException)
            {
                continue;
            }
        }

        return result;
    }

    /// <summary>
    /// Gets all loaded assemblies recursively, including assemblies found in bundles or packages.
    /// </summary>
    public async Task<IList<LoadedAssembly>> GetAllAssembliesAsync()
    {
        var results = new List<LoadedAssembly>(assemblies.Length);

        foreach (var asm in assemblies)
        {
            LoadResult result;
            try
            {
                result = await asm.GetLoadResultAsync().ConfigureAwait(false);
            }
            catch
            {
                results.Add(asm);
                continue;
            }
            if (result.Package != null)
            {
                AddDescendants(result.Package.RootFolder);
            }
            else if (result.MetadataFile != null)
            {
                results.Add(asm);
            }
        }

        void AddDescendants(PackageFolder folder)
        {
            foreach (var subFolder in folder.Folders)
            {
                AddDescendants(subFolder);
            }

            foreach (var entry in folder.Entries)
            {
                if (!entry.Name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) && !entry.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var asm = folder.ResolveFileName(entry.Name);
                if (asm == null)
                {
                    continue;
                }

                results.Add(asm);
            }
        }

        return results;
    }
}

