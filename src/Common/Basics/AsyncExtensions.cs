namespace SpaceEngineers.Core.Basics;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public static class AsyncExtensions
{
    public static Task WhenAll(this IEnumerable<Task> source)
    {
        return Task.WhenAll(source);
    }

    public static Task<TResult[]> WhenAll<TResult>(this IEnumerable<Task<TResult>> source)
    {
        return Task.WhenAll(source);
    }

    public static async Task<IEnumerable<T>> AsEnumerable<T>(this IAsyncEnumerable<T> source, CancellationToken token)
    {
        var list = new List<T>();

        var asyncSource = source
            .WithCancellation(token)
            .ConfigureAwait(false);

        await foreach (var item in asyncSource)
        {
            list.Add(item);
        }

        return list;
    }
}