namespace SpaceEngineers.Core.TracingEndpoint.MessageHandlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Contract.Messages;
    using DataAccess.Api.Transaction;
    using Domain;
    using GenericEndpoint.Api.Abstractions;

    [Component(EnLifestyle.Transient)]
    internal class CaptureTraceMessageHandler : IMessageHandler<CaptureTrace>,
                                                ICollectionResolvable<IMessageHandler<CaptureTrace>>
    {
        private readonly IDatabaseContext _databaseContext;

        public CaptureTraceMessageHandler(IDatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public Task Handle(CaptureTrace command, CancellationToken token)
        {
            var capturedMessage = new CapturedMessage(command.IntegrationMessage, command.Exception?.ToString());

            return _databaseContext.Track(capturedMessage, token);
        }
    }
}