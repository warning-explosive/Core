namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Messaging;
    using UnitOfWork;

    [Component(EnLifestyle.Scoped)]
    internal class MessagesCollector : IMessagesCollector,
                                       IResolvable<IMessagesCollector>
    {
        private readonly IIntegrationUnitOfWork _unitOfWork;

        public MessagesCollector(IIntegrationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task Collect(IntegrationMessage message, CancellationToken token)
        {
            return _unitOfWork.OutboxStorage.Add(message, token);
        }
    }
}