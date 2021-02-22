namespace SpaceEngineers.Core.GenericHost.Internals
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoWiringApi.Abstractions;

    internal interface IRunnableEndpoint : IAsyncDisposable, IResolvable
    {
        Task StartAsync(CancellationToken token);

        Task StopAsync();
    }
}