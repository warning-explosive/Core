namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Extensions for operations with System.IO.Stream class
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Read all containing bytes asynchronously and convert to encoded string
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="encoding">String encoding</param>
        /// <returns>Encoded bytes</returns>
        public static async Task<string> ReadAllAsync(this Stream stream, Encoding encoding)
        {
            return encoding.GetString(await stream.ReadAllAsync().ConfigureAwait(false));
        }

        /// <summary>
        /// Flush stream and write string asynchronously
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="serialized">String for writing</param>
        /// <param name="encoding">String encoding</param>
        /// <returns>Task-object</returns>
        public static async Task OverWriteAllAsync(this Stream stream, string serialized, Encoding encoding)
        {
            await stream.OverWriteAllAsync(encoding.GetBytes(serialized)).ConfigureAwait(false);
        }

        /// <summary>
        /// Read all containing bytes into buffer asynchronously
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>Filled buffer</returns>
        public static async Task<byte[]> ReadAllAsync(this Stream stream)
        {
            var bytes = (int)stream.Length;
            var buffer = new byte[bytes];

            var offset = 0;
            stream.Position = offset;
            var memory = new Memory<byte>(buffer, offset, bytes);

            await stream.ReadAsync(memory).ConfigureAwait(false);

            return buffer;
        }

        /// <summary>
        /// Flush stream and write bytes asynchronously
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="bytes">Bytes for writing</param>
        /// <returns>Task-object</returns>
        public static async Task OverWriteAllAsync(this Stream stream, byte[] bytes)
        {
            await stream.FlushAsync().ConfigureAwait(false);

            var offset = 0;
            stream.Position = offset;
            var memory = new ReadOnlyMemory<byte>(bytes, offset, bytes.Length);

            stream.SetLength(bytes.Length);
            await stream.WriteAsync(memory).ConfigureAwait(false);
        }
    }
}