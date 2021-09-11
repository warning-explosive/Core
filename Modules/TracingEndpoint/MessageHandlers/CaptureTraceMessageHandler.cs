namespace SpaceEngineers.Core.TracingEndpoint.MessageHandlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Contract.Messages;
    using DataAccess.Api.Abstractions;
    using Domain;
    using GenericEndpoint.Api.Abstractions;

    [Component(EnLifestyle.Transient)]
    internal class CaptureTraceMessageHandler : IMessageHandler<CaptureTrace>,
                                                ICollectionResolvable<IMessageHandler<CaptureTrace>>
    {
        private readonly IDatabaseContext _databaseContext;

        // TODO: #152 - remove IIntegrationContext from the handle method and inject it where it is necessary
        private IIntegrationContext _integrationContext;

        public CaptureTraceMessageHandler(
            IDatabaseContext databaseContext,
            IIntegrationContext integrationContext)
        {
            _databaseContext = databaseContext;
            _integrationContext = integrationContext;
        }

        public Task Handle(CaptureTrace command, IIntegrationContext context, CancellationToken token)
        {
            var capturedMessage = new CapturedMessage(command.IntegrationMessage, command.Exception?.ToString());

            return _databaseContext.Track(capturedMessage, token);
        }
    }
}