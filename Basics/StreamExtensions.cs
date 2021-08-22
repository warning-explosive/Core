namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;
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
        /// <param name="token">Cancellation token</param>
        /// <returns>Encoded bytes</returns>
        public static async Task<string> ReadAllAsync(this Stream stream, Encoding encoding, CancellationToken token)
        {
            return encoding.GetString(await stream.ReadAllAsync(token).ConfigureAwait(false));
        }

        /// <summary>
        /// Flush stream and write string asynchronously
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="serialized">String for writing</param>
        /// <param name="encoding">String encoding</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Task-object</returns>
        public static async Task OverWriteAllAsync(this Stream stream, string serialized, Encoding encoding, CancellationToken token)
        {
            await stream.OverWriteAllAsync(encoding.GetBytes(serialized), token).ConfigureAwait(false);
        }

        /// <summary>
        /// Read all containing bytes into buffer asynchronously
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Filled buffer</returns>
        public static async Task<byte[]> ReadAllAsync(this Stream stream, CancellationToken token)
        {
            var bytes = (int)stream.Length;
            var buffer = new byte[bytes];

            var offset = 0;
            stream.Position = offset;
            var memory = new Memory<byte>(buffer, offset, bytes);

            await stream.ReadAsync(memory, token).ConfigureAwait(false);

            return buffer;
        }

        /// <summary>
        /// Flush stream and write bytes asynchronously
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="bytes">Bytes for writing</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Task-object</returns>
        public static async Task OverWriteAllAsync(this Stream stream, byte[] bytes, CancellationToken token)
        {
            await stream.FlushAsync(token).ConfigureAwait(false);

            var offset = 0;
            stream.Position = offset;
            var memory = new ReadOnlyMemory<byte>(bytes, offset, bytes.Length);

            stream.SetLength(bytes.Length);
            await stream.WriteAsync(memory, token).ConfigureAwait(false);
        }
    }
}