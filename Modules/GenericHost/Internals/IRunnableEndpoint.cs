namespace SpaceEngineers.Core.GenericHost.Internals
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoWiring.Api.Abstractions;

    internal interface IRunnableEndpoint : IAsyncDisposable, IResolvable
    {
        Task StartAsync(CancellationToken token);

        Task StopAsync();
    }
}