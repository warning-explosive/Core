namespace SpaceEngineers.Core.GenericHost.Internals
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoWiringApi.Attributes;
    using Core.GenericEndpoint.Abstractions;

    [Unregistered]
    internal class CompositeEndpoint : ICompositeEndpoint, IRunnableEndpoint
    {
        private CancellationTokenSource? _cts;

        public CompositeEndpoint(params IGenericEndpoint[] endpoints)
        {
            Endpoints = endpoints;
        }

        public IReadOnlyCollection<IGenericEndpoint> Endpoints { get; }

        private CancellationToken Token => _cts?.Token ?? CancellationToken.None;

        public ValueTask DisposeAsync()
        {
            return new ValueTask(StopAsync());
        }

        public async Task StartAsync(CancellationToken token)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(token);

            foreach (var endpoint in Endpoints)
            {
                await ((IRunnableEndpoint)endpoint).StartAsync(token).ConfigureAwait(false);
            }
        }

        public async Task StopAsync()
        {
            foreach (var endpoint in Endpoints)
            {
                await ((IRunnableEndpoint)endpoint).StartAsync(Token).ConfigureAwait(false);
            }
        }
    }
}