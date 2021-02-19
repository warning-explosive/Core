namespace SpaceEngineers.Core.GenericHost.InternalAbstractions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    internal interface IRunnableEndpoint : IAsyncDisposable
    {
        Task StartAsync(CancellationToken token);

        Task StopAsync();
    }
}