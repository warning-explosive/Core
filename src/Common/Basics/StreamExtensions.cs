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
        /// Converts bytes to memory steam
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>MemoryStream</returns>
        public static MemoryStream AsMemoryStream(this ReadOnlySpan<byte> bytes)
        {
            var memoryStream = new MemoryStream(bytes.Length);

            memoryStream.Write(bytes);
            memoryStream.Position = 0;

            return memoryStream;
        }

        /// <summary>
        /// Converts MemoryStream to bytes
        /// </summary>
        /// <param name="memoryStream">MemoryStream</param>
        /// <returns>Bytes</returns>
        public static ReadOnlyMemory<byte> AsBytes(this MemoryStream memoryStream)
        {
            return memoryStream.TryGetBuffer(out var arraySegment)
                ? arraySegment
                : throw new InvalidOperationException("Unable to extract buffer from the memoryStream");
        }

        /// <summary>
        /// Converts stream to bytes
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>Bytes</returns>
        public static ReadOnlyMemory<byte> AsBytes(this Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);

                return memoryStream.AsBytes();
            }
        }

        /// <summary>
        /// Converts stream to bytes
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Bytes</returns>
        public static async Task<ReadOnlyMemory<byte>> AsBytes(this Stream stream, CancellationToken token)
        {
            using (var memoryStream = new MemoryStream())
            {
                await stream
                    .CopyToAsync(memoryStream, token)
                    .ConfigureAwait(false);

                return memoryStream.AsBytes();
            }
        }

        /// <summary>
        /// Converts stream to encoded string
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="encoding">Encoding</param>
        /// <returns>String</returns>
        public static string AsString(
            this Stream stream,
            Encoding encoding)
        {
            var bytes = stream.AsBytes();

            return encoding.GetString(bytes.Span);
        }

        /// <summary>
        /// Converts stream to encoded string
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="encoding">Encoding</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>String</returns>
        public static async Task<string> AsString(
            this Stream stream,
            Encoding encoding,
            CancellationToken token)
        {
            var bytes = await stream
                .AsBytes(token)
                .ConfigureAwait(false);

            return encoding.GetString(bytes.Span);
        }

        /// <summary>
        /// Flushes stream and writes bytes
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="bytes">Bytes</param>
        public static void Overwrite(
            this Stream stream,
            ReadOnlySpan<byte> bytes)
        {
            stream.Flush();

            stream.Position = 0;
            stream.SetLength(bytes.Length);

            stream.Write(bytes);

            stream.Position = 0;
        }

        /// <summary>
        /// Flushes stream and writes bytes
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="bytes">Bytes</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        public static async Task Overwrite(
            this Stream stream,
            ReadOnlyMemory<byte> bytes,
            CancellationToken token)
        {
            await stream
                .FlushAsync(token)
                .ConfigureAwait(false);

            stream.Position = 0;
            stream.SetLength(bytes.Length);

            await stream
                .WriteAsync(bytes, token)
                .ConfigureAwait(false);

            stream.Position = 0;
        }
    }
}