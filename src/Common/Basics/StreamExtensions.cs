namespace SpaceEngineers.Core.Basics;

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public static class StreamExtensions
{
    public static MemoryStream AsMemoryStream(this ReadOnlySpan<byte> bytes)
    {
        var memoryStream = new MemoryStream(bytes.Length);

        memoryStream.Write(bytes);
        memoryStream.Position = 0;

        return memoryStream;
    }

    public static ReadOnlyMemory<byte> AsBytes(this MemoryStream memoryStream)
    {
        return memoryStream.TryGetBuffer(out var arraySegment)
            ? arraySegment
            : throw new InvalidOperationException("Unable to extract buffer from the memoryStream");
    }

    public static ReadOnlyMemory<byte> AsBytes(this Stream stream)
    {
        using (var memoryStream = new MemoryStream())
        {
            stream.CopyTo(memoryStream);

            return memoryStream.AsBytes();
        }
    }

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

    public static string AsString(this Stream stream, Encoding encoding)
    {
        var bytes = stream.AsBytes();

        return encoding.GetString(bytes.Span);
    }

    public static async Task<string> AsString(this Stream stream, Encoding encoding, CancellationToken token)
    {
        var bytes = await stream
            .AsBytes(token)
            .ConfigureAwait(false);

        return encoding.GetString(bytes.Span);
    }

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