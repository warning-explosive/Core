namespace SpaceEngineers.Core.TracingEndpoint.MessageHandlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Contract;
    using DataAccess.Api.Persisting;
    using DataAccess.Api.Transaction;
    using DatabaseModel;
    using GenericEndpoint.Api.Abstractions;

    [Component(EnLifestyle.Transient)]
    internal class CaptureTraceMessageHandler : IMessageHandler<CaptureTrace>,
                                                IResolvable<IMessageHandler<CaptureTrace>>
    {
        private readonly IDatabaseContext _databaseContext;

        public CaptureTraceMessageHandler(IDatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public Task Handle(CaptureTrace command, CancellationToken token)
        {
            var message = IntegrationMessage.Build(command.SerializedMessage);

            var capturedMessage = new CapturedMessage(Guid.NewGuid(), message, command.Exception?.ToString());

            return _databaseContext
               .Write<CapturedMessage>()
               .Insert(new[] { capturedMessage }, EnInsertBehavior.Default, token);
        }
    }
}