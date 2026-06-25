#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;

using ICSharpCode.Decompiler.Metadata;

namespace ICSharpCode.ILSpy.Controls.FileLoaders;

public sealed class LoadResult
{
    public MetadataFile? MetadataFile { get; init; }
    public Exception? FileLoadException { get; init; }
    public LoadedPackage? Package { get; init; }

    public bool IsSuccess => FileLoadException == null;
}

public record FileLoadContext(bool ApplyWinRTProjections, LoadedAssembly? ParentBundle);

public interface IFileLoader
{
    Task<LoadResult?> Load(string fileName, Stream stream, FileLoadContext context);
}
