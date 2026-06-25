using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.Metadata;
using System.Buffers;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using K4os.Compression.LZ4;
using static ICSharpCode.Decompiler.Metadata.MetadataFile;

namespace ICSharpCode.ILSpy.Controls.FileLoaders;

public sealed class FileLoaderRegistry
{
    readonly List<IFileLoader> registeredLoaders = [];

    public IReadOnlyList<IFileLoader> RegisteredLoaders => registeredLoaders;

    public void Register(IFileLoader loader)
    {
        ArgumentNullException.ThrowIfNull(loader);

        registeredLoaders.Add(loader);
    }

    public FileLoaderRegistry()
    {
        Register(new XamarinCompressedFileLoader());
        Register(new WebCilFileLoader());
        Register(new MetadataFileLoader());
        Register(new BundleFileLoader()); // bundles are PE files with a special signature, prefer over normal PE files
        Register(new PEFileLoader()); // prefer PE format over archives, because ZIP has no fixed header
        Register(new ArchiveFileLoader());
    }
}


public sealed class XamarinCompressedFileLoader : IFileLoader
{
    public async Task<LoadResult?> Load(string fileName, Stream stream, FileLoadContext context)
    {
        const uint CompressedDataMagic = 0x5A4C4158; // Magic used for Xamarin compressed module header ('XALZ', little-endian)
        using var fileReader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        // Read compressed file header
        var magic = fileReader.ReadUInt32();
        if (magic != CompressedDataMagic)
            return null;
        _ = fileReader.ReadUInt32(); // skip index into descriptor table, unused
        int uncompressedLength = (int)fileReader.ReadUInt32();
        int compressedLength = (int)stream.Length;  // Ensure we read all of compressed data
        ArrayPool<byte> pool = ArrayPool<byte>.Shared;
        var src = pool.Rent(compressedLength);
        var dst = pool.Rent(uncompressedLength);
        try
        {
            // fileReader stream position is now at compressed module data
            await stream.ReadAsync(src, 0, compressedLength).ConfigureAwait(false);
            // Decompress
            LZ4Codec.Decode(src, 0, compressedLength, dst, 0, uncompressedLength);
            // Load module from decompressed data buffer
            using var uncompressedStream = new MemoryStream(dst, writable: false);
            MetadataReaderOptions options = context.ApplyWinRTProjections
                ? MetadataReaderOptions.ApplyWindowsRuntimeProjections
                : MetadataReaderOptions.None;

            return new LoadResult
            {
                MetadataFile = new PEFile(fileName, uncompressedStream, PEStreamOptions.PrefetchEntireImage, metadataOptions: options)
            };
        }
        finally
        {
            pool.Return(dst);
            pool.Return(src);
        }
    }
}
public sealed class WebCilFileLoader : IFileLoader
{
    public Task<LoadResult?> Load(string fileName, Stream stream, FileLoadContext settings)
    {
        if (settings.ParentBundle != null)
        {
            return Task.FromResult<LoadResult?>(null);
        }

        MetadataReaderOptions options = settings.ApplyWinRTProjections
                        ? MetadataReaderOptions.ApplyWindowsRuntimeProjections
                        : MetadataReaderOptions.None;

        var wasm = WebCilFile.FromFile(fileName, options);
        var result = wasm != null ? new LoadResult { MetadataFile = wasm } : null;
        return Task.FromResult(result);
    }
}
public sealed class MetadataFileLoader : IFileLoader
{
    public Task<LoadResult?> Load(string fileName, Stream stream, FileLoadContext settings)
    {
        try
        {
            var kind = Path.GetExtension(fileName).Equals(".pdb", StringComparison.OrdinalIgnoreCase)
                ? MetadataFileKind.ProgramDebugDatabase : MetadataFileKind.Metadata;
            var metadata = MetadataReaderProvider.FromMetadataStream(stream, MetadataStreamOptions.PrefetchMetadata | MetadataStreamOptions.LeaveOpen);
            var metadataFile = new MetadataFile(kind, fileName, metadata);
            return Task.FromResult<LoadResult?>(new LoadResult { MetadataFile = metadataFile });
        }
        catch (BadImageFormatException)
        {
            return Task.FromResult<LoadResult?>(null);
        }
    }
}
public sealed class BundleFileLoader : IFileLoader
{
    public Task<LoadResult?> Load(string fileName, Stream stream, FileLoadContext settings)
    {
        if (settings.ParentBundle != null)
        {
            return Task.FromResult<LoadResult?>(null);
        }

        var bundle = LoadedPackage.FromBundle(fileName);
        var result = bundle != null ? new LoadResult { Package = bundle } : null;
        return Task.FromResult(result);
    }
}

public sealed class PEFileLoader : IFileLoader
{
    public async Task<LoadResult?> Load(string fileName, Stream stream, FileLoadContext context)
    {
        if (stream.Length < 2 || stream.ReadByte() != 'M' || stream.ReadByte() != 'Z')
        {
            return null;
        }

        return await LoadPEFile(fileName, stream, context).ConfigureAwait(false);
    }

    public static Task<LoadResult> LoadPEFile(string fileName, Stream stream, FileLoadContext context)
    {
        MetadataReaderOptions options = context.ApplyWinRTProjections
            ? MetadataReaderOptions.ApplyWindowsRuntimeProjections
            : MetadataReaderOptions.None;
        stream.Position = 0;
        PEFile module = new PEFile(fileName, stream, PEStreamOptions.PrefetchEntireImage | PEStreamOptions.LeaveOpen, metadataOptions: options);
        return Task.FromResult(new LoadResult { MetadataFile = module });
    }
}
public sealed class ArchiveFileLoader : IFileLoader
{
    public Task<LoadResult?> Load(string fileName, Stream stream, FileLoadContext settings)
    {
        if (settings.ParentBundle != null)
        {
            return Task.FromResult<LoadResult?>(null);
        }

        try
        {
            var zip = LoadedPackage.FromZipFile(fileName);
            var result = zip != null ? new LoadResult { Package = zip } : null;
            return Task.FromResult(result);
        }
        catch (InvalidDataException)
        {
            return Task.FromResult<LoadResult?>(null);
        }
    }
}
