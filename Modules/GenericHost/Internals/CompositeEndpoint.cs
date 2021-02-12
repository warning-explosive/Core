namespace SpaceEngineers.Core.GenericHost.Internals
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;

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

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            foreach (var endpoint in Endpoints.OfType<IRunnableEndpoint>())
            {
                await endpoint.StartAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task StopAsync()
        {
            foreach (var endpoint in Endpoints.OfType<IRunnableEndpoint>())
            {
                await endpoint.StartAsync(Token).ConfigureAwait(false);
            }
        }
    }
}