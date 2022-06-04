namespace SpaceEngineers.Core.Basics
{
    using System.IO;
    using System.IO.Compression;

    /// <summary>
    /// Compression extensions
    /// </summary>
    public static class CompressionExtensions
    {
        /// <summary>
        /// Compresses data represented as byte-array
        /// </summary>
        /// <param name="bytes">Decompressed data represented as byte-array</param>
        /// <returns>Compressed data represented as byte-array</returns>
        public static byte[] Compress(this byte[] bytes)
        {
            using (var to = new MemoryStream())
            using (var zipStream = new GZipStream(to, CompressionMode.Compress))
            {
                zipStream.Write(bytes, 0, bytes.Length);

                zipStream.Close();

                return to.ToArray();
            }
        }

        /// <summary>
        /// Decompresses data represented as byte-array
        /// </summary>
        /// <param name="bytes">Compressed data represented as byte-array</param>
        /// <returns>Decompressed data represented as byte-array</returns>
        public static byte[] Decompress(this byte[] bytes)
        {
            using (var to = new MemoryStream())
            using (var from = new MemoryStream(bytes))
            using (var zipStream = new GZipStream(from, CompressionMode.Decompress))
            {
                zipStream.CopyTo(to);

                zipStream.Close();

                return to.ToArray();
            }
        }
    }
}