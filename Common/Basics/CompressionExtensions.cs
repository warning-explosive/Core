namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.IO;
    using System.IO.Compression;

    /// <summary>
    /// Compression extensions
    /// </summary>
    public static class CompressionExtensions
    {
        /// <summary>
        /// Compresses data
        /// </summary>
        /// <param name="bytes">Decompressed data</param>
        /// <returns>Compressed data</returns>
        public static Memory<byte> Compress(this ReadOnlySpan<byte> bytes)
        {
            using (var to = new MemoryStream())
            using (var zipStream = new GZipStream(to, CompressionMode.Compress, leaveOpen: false))
            {
                zipStream.Write(bytes);

                zipStream.Close(); // committing changes into underlying stream

                return to.AsBytes();
            }
        }

        /// <summary>
        /// Decompresses data
        /// </summary>
        /// <param name="bytes">Compressed data</param>
        /// <returns>Decompressed data</returns>
        public static Memory<byte> Decompress(this ReadOnlySpan<byte> bytes)
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
}