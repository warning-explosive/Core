namespace SpaceEngineers.Core.Basics;

using System;
using System.IO;
using System.IO.Compression;

public static class CompressionExtensions
{
    public static ReadOnlyMemory<byte> Compress(this ReadOnlySpan<byte> bytes)
    {
        using (var to = new MemoryStream())
        using (var zipStream = new GZipStream(to, CompressionMode.Compress, leaveOpen: false))
        {
            zipStream.Write(bytes);

            zipStream.Close(); // committing changes into underlying stream

            return to.AsBytes();
        }
    }

    public static ReadOnlyMemory<byte> Decompress(this ReadOnlySpan<byte> bytes)
    {
        using (var from = bytes.AsMemoryStream())
        using (var zipStream = new GZipStream(from, CompressionMode.Decompress, leaveOpen: false))
        {
            try
            {
                return zipStream.AsBytes();
            }
            finally
            {
                zipStream.Close();
            }
        }
    }
}