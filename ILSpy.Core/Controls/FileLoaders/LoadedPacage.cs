#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Metadata;
namespace ICSharpCode.ILSpy.Controls.FileLoaders;

/// <summary>
/// NuGet package or .NET bundle:
/// </summary>
public class LoadedPackage
{
    public enum PackageKind
    {
        Zip,
        Bundle,
    }

    /// <summary>
    /// Gets the LoadedAssembly instance representing this bundle.
    /// </summary>
    internal LoadedAssembly? LoadedAssembly { get; set; }

    public PackageKind Kind { get; }

    public SingleFileBundle.Header BundleHeader { get; set; }

    /// <summary>
    /// List of all entries, including those in sub-directories within the package.
    /// </summary>
    public IReadOnlyList<PackageEntry> Entries { get; }

    public PackageFolder RootFolder { get; }

    public LoadedPackage(PackageKind kind, IEnumerable<PackageEntry> entries)
    {
        Kind = kind;
        Entries = entries.ToArray();
        var topLevelEntries = new List<PackageEntry>();
        var folders = new Dictionary<string, PackageFolder>();
        var rootFolder = new PackageFolder(this, null, "");
        folders.Add("", rootFolder);
        foreach (var entry in Entries)
        {
            var (dirname, filename) = SplitName(entry.Name);
            if (!string.IsNullOrEmpty(filename))
            {
                GetFolder(dirname).Entries.Add(new FolderEntry(filename, entry));
            }
        }
        RootFolder = rootFolder;

        static (string, string) SplitName(string filename)
        {
            int pos = filename.LastIndexOfAny(new char[] { '/', '\\' });
            if (pos == -1)
            {
                return ("", filename); // file in root
            }
            else
            {
                return (filename.Substring(0, pos), filename.Substring(pos + 1));
            }
        }

        PackageFolder GetFolder(string name)
        {
            if (folders.TryGetValue(name, out var result))
            {
                return result;
            }

            var (dirname, basename) = SplitName(name);
            PackageFolder parent = GetFolder(dirname);
            result = new PackageFolder(this, parent, basename);
            parent.Folders.Add(result);
            folders.Add(name, result);
            return result;
        }
    }

    public static LoadedPackage FromZipFile(string file)
    {
        Debug.WriteLine($"LoadedPackage.FromZipFile({file})");
        using var archive = ZipFile.OpenRead(file);
        return new LoadedPackage(PackageKind.Zip,
            archive.Entries.Select(entry => new ZipFileEntry(file, entry)));
    }

    /// <summary>
    /// Load a .NET single-file bundle.
    /// </summary>
    public static LoadedPackage? FromBundle(string fileName)
    {
        using var memoryMappedFile = MemoryMappedFile.CreateFromFile(fileName, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
        var view = memoryMappedFile.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
        try
        {
            if (!SingleFileBundle.IsBundle(view, out long bundleHeaderOffset))
            {
                return null;
            }

            var manifest = SingleFileBundle.ReadManifest(view, bundleHeaderOffset);
            var entries = manifest.Entries.Select(e => new BundleEntry(fileName, view, e)).ToList();
            var result = new LoadedPackage(PackageKind.Bundle, entries);
            result.BundleHeader = manifest;
            view = null; // don't dispose the view, we're still using it in the bundle entries
            return result;
        }
        catch (InvalidDataException)
        {
            return null;
        }
        finally
        {
            view?.Dispose();
        }
    }

    /// <summary>
    /// Entry inside a package folder. Effectively renames the entry.
    /// </summary>
    sealed class FolderEntry(string name, PackageEntry originalEntry) : PackageEntry
    {
        readonly PackageEntry originalEntry = originalEntry;
        public override string Name { get; } = name;
        public override string FullName => originalEntry.Name;

        public override ManifestResourceAttributes Attributes => originalEntry.Attributes;
        public override string PackageQualifiedFileName => originalEntry.PackageQualifiedFileName;
        public override ResourceType ResourceType => originalEntry.ResourceType;
        public override Stream? TryOpenStream() => originalEntry.TryOpenStream();
        public override long? TryGetLength() => originalEntry.TryGetLength();
    }

    sealed class ZipFileEntry(string zipFile, ZipArchiveEntry entry) : PackageEntry
    {
        readonly string zipFile = zipFile;
        public override string Name { get; } = entry.FullName;
        public override string PackageQualifiedFileName => $"zip://{zipFile};{Name}";

        public override string FullName => Name;

        public override Stream? TryOpenStream()
        {
            Debug.WriteLine("Decompress " + Name);
            using var archive = ZipFile.OpenRead(zipFile);
            var entry = archive.GetEntry(Name);
            if (entry == null)
            {
                return null;
            }

            var memoryStream = new MemoryStream();
            using (var s = entry.Open())
            {
                s.CopyTo(memoryStream);
            }
            memoryStream.Position = 0;
            return memoryStream;
        }

        public override long? TryGetLength()
        {
            Debug.WriteLine("TryGetLength " + Name);
            using var archive = ZipFile.OpenRead(zipFile);
            var entry = archive.GetEntry(Name);
            if (entry == null)
            {
                return null;
            }

            return entry.Length;
        }
    }

    sealed class BundleEntry(string bundleFile, MemoryMappedViewAccessor view, SingleFileBundle.Entry entry) : PackageEntry
    {
        readonly string bundleFile = bundleFile;
        readonly MemoryMappedViewAccessor view = view;
        readonly SingleFileBundle.Entry entry = entry;

        public override string Name => entry.RelativePath;
        public override string FullName => Name;
        public override string PackageQualifiedFileName => $"bundle://{bundleFile};{Name}";

        public override Stream TryOpenStream()
        {
            Debug.WriteLine("Open bundle member " + Name);

            if (entry.CompressedSize == 0)
            {
                return new UnmanagedMemoryStream(view.SafeMemoryMappedViewHandle, entry.Offset, entry.Size);
            }
            else
            {
                Stream compressedStream = new UnmanagedMemoryStream(view.SafeMemoryMappedViewHandle, entry.Offset, entry.CompressedSize);
                using var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress);
                Stream decompressedStream = new MemoryStream((int)entry.Size);
                deflateStream.CopyTo(decompressedStream);
                if (decompressedStream.Length != entry.Size)
                {
                    throw new InvalidDataException($"Corrupted single-file entry '{entry.RelativePath}'. Declared decompressed size '{entry.Size}' is not the same as actual decompressed size '{decompressedStream.Length}'.");
                }

                decompressedStream.Seek(0, SeekOrigin.Begin);
                return decompressedStream;
            }
        }

        public override long? TryGetLength() => entry.Size;
    }
}

public abstract class PackageEntry : Resource
{
    /// <summary>
    /// Gets the file name of the entry (may include path components, relative to the package root).
    /// </summary>
    public abstract override string Name { get; }

    /// <summary>
    /// Gets the full file name including the full file name of the package (prefixed with e.g., bundle:// or zip://).
    /// </summary>
    public abstract string PackageQualifiedFileName { get; }

    /// <summary>
    /// Gets the full name of the file name relative to the package root.
    /// </summary>
    public abstract string FullName { get; }
}

public sealed class PackageFolder : IAssemblyResolver
{
    /// <summary>
    /// Gets the short name of the folder.
    /// </summary>
    public string Name { get; }

    readonly LoadedPackage package;

    internal PackageFolder(LoadedPackage package, PackageFolder? parent, string name)
    {
        this.package = package;
        Parent = parent;
        Name = name;
    }

    public PackageFolder? Parent { get; }
    public List<PackageFolder> Folders { get; } = [];
    public List<PackageEntry> Entries { get; } = [];

    public MetadataFile? Resolve(IAssemblyReference reference)
    {
        var asm = ResolveFileName(reference.Name + ".dll");
        if (asm != null)
        {
            return asm.GetMetadataFileOrNull();
        }
        return Parent?.Resolve(reference);
    }

    public Task<MetadataFile?> ResolveAsync(IAssemblyReference reference)
    {
        var asm = ResolveFileName(reference.Name + ".dll");
        if (asm != null)
        {
            return asm.GetMetadataFileOrNullAsync();
        }
        if (Parent != null)
        {
            return Parent.ResolveAsync(reference);
        }
        return Task.FromResult<MetadataFile?>(null);
    }

    public MetadataFile? ResolveModule(MetadataFile mainModule, string moduleName)
    {
        var asm = ResolveFileName(moduleName + ".dll");
        if (asm != null)
        {
            return asm.GetMetadataFileOrNull();
        }
        return Parent?.ResolveModule(mainModule, moduleName);
    }

    public Task<MetadataFile?> ResolveModuleAsync(MetadataFile mainModule, string moduleName)
    {
        var asm = ResolveFileName(moduleName + ".dll");
        if (asm != null)
        {
            return asm.GetMetadataFileOrNullAsync();
        }
        if (Parent != null)
        {
            return Parent.ResolveModuleAsync(mainModule, moduleName);
        }
        return Task.FromResult<MetadataFile?>(null);
    }

    readonly Dictionary<string, LoadedAssembly?> assemblies = new(StringComparer.OrdinalIgnoreCase);

    public LoadedAssembly? ResolveFileName(string name)
    {
        if (package.LoadedAssembly == null)
        {
            return null;
        }

        lock (assemblies)
        {
            if (assemblies.TryGetValue(name, out var asm))
            {
                return asm;
            }

            var entry = Entries.FirstOrDefault(e => string.Equals(name, e.Name, StringComparison.OrdinalIgnoreCase));
            if (entry != null)
            {
                asm = new LoadedAssembly(
                    package.LoadedAssembly.AssemblyList,
                    entry.Name,
                    stream: entry.TryOpenStream()!
                );
            }
            else
            {
                asm = null;
            }
            assemblies.Add(name, asm);
            return asm;
        }
    }
}
