namespace SpaceEngineers.Core.Modules.Test.MessageHandlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using GenericEndpoint.Api.Abstractions;
    using Messages;

    [Component(EnLifestyle.Transient)]
    internal class MakeSingleQueryCommandHandler : IMessageHandler<MakeSingleQueryCommand>,
                                                   ICollectionResolvable<IMessageHandler<MakeSingleQueryCommand>>
    {
        public Task Handle(MakeSingleQueryCommand message, IIntegrationContext context, CancellationToken token)
        {
            var query = new Query(message.Id);

            return context.Request<Query, Reply>(query, token);
        }
    }
}