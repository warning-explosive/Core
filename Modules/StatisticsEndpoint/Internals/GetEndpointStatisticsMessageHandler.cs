namespace SpaceEngineers.Core.StatisticsEndpoint.Internals
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Contract.Messages;
    using GenericEndpoint.Api.Abstractions;

    [Component(EnLifestyle.Transient)]
    internal class GetEndpointStatisticsMessageHandler : IMessageHandler<GetEndpointStatistics>,
                                                         ICollectionResolvable<IMessageHandler<GetEndpointStatistics>>
    {
        public Task Handle(GetEndpointStatistics message, IIntegrationContext context, CancellationToken token)
        {
            // TODO: #112 - add metrics
            var reply = new EndpointStatisticsReply(message.EndpointIdentity);

            return context.Reply(message, reply, token);
        }
    }
}