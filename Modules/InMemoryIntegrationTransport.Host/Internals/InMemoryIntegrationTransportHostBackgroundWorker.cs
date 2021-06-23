namespace SpaceEngineers.Core.InMemoryIntegrationTransport.Host.Internals
{
    using System.Threading;
    using System.Threading.Tasks;
    using GenericHost.Api.Abstractions;

    internal class InMemoryIntegrationTransportHostBackgroundWorker : IHostBackgroundWorker
    {
        private readonly InMemoryIntegrationTransport _transport;

        public InMemoryIntegrationTransportHostBackgroundWorker(InMemoryIntegrationTransport transport)
        {
            _transport = transport;
        }

        public Task Run(CancellationToken token)
        {
            return _transport.StartMessageProcessing(token);
        }
    }
}