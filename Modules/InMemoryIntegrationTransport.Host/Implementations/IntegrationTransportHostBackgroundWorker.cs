namespace SpaceEngineers.Core.InMemoryIntegrationTransport.Host.Implementations
{
    using System.Threading;
    using System.Threading.Tasks;
    using GenericHost.Api.Abstractions;
    using IntegrationTransport.Api.Abstractions;

    internal class IntegrationTransportHostBackgroundWorker : IHostBackgroundWorker
    {
        private readonly IIntegrationTransport _transport;

        public IntegrationTransportHostBackgroundWorker(IIntegrationTransport transport)
        {
            _transport = transport;
        }

        public Task Run(CancellationToken token)
        {
            return _transport.StartBackgroundMessageProcessing(token);
        }
    }
}