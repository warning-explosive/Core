namespace SpaceEngineers.Core.AuthorizationEndpoint.MessageHandlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Contract.Messages;
    using GenericEndpoint.Api.Abstractions;

    [Component(EnLifestyle.Transient)]
    internal class CreateUserMessageHandler : IMessageHandler<CreateUser>,
                                              ICollectionResolvable<IMessageHandler<CreateUser>>
    {
        private readonly IIntegrationContext _integrationContext;

        public CreateUserMessageHandler(IIntegrationContext integrationContext)
        {
            _integrationContext = integrationContext;
        }

        public Task Handle(CreateUser message, CancellationToken token)
        {
            throw new System.NotImplementedException("#165");
        }
    }
}