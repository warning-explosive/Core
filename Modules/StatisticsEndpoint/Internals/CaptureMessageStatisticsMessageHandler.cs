namespace SpaceEngineers.Core.StatisticsEndpoint.Internals
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Contract.Messages;
    using CrossCuttingConcerns.Api.Abstractions;
    using DataAccess.Contract.Abstractions;
    using GenericEndpoint.Api;
    using GenericEndpoint.Api.Abstractions;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Messaging;
    using Model;

    [Component(EnLifestyle.Transient)]
    internal class CaptureMessageStatisticsMessageHandler : MessageHandlerBase<CaptureMessageStatistics>
    {
        private readonly IReadRepository<EndpointStatistics> _repository;
        private readonly IJsonSerializer _serializer;

        public CaptureMessageStatisticsMessageHandler(
            IReadRepository<EndpointStatistics> repository,
            IJsonSerializer serializer)
        {
            _repository = repository;
            _serializer = serializer;
        }

        public override Task Handle(CaptureMessageStatistics message, IIntegrationContext context, CancellationToken token)
        {
            var originator = message.GeneralMessage.ReadRequiredHeader<EndpointIdentity>(IntegrationMessageHeader.SentFrom);

            var endpointStatistics = _repository
                .All()
                .SingleOrDefault(it => it.EndpointLogicalName == originator.LogicalName
                                       && it.EndpointInstanceName == originator.InstanceName)
                ?? new EndpointStatistics(originator);

            var messageInfo = MessageInfo.FromIntegrationMessage(message.GeneralMessage, _serializer);

            if (message.Exception == null)
            {
                endpointStatistics.SuccessfulMessages.Add(messageInfo);
            }
            else
            {
                var failedMessage = new FailedMessage(messageInfo, message.Exception);
                endpointStatistics.FailedMessages.Add(failedMessage);
            }

            return Task.CompletedTask;
        }
    }
}