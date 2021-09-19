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
    internal class RequestQueryCommandHandler : IMessageHandler<RequestQueryCommand>,
                                                ICollectionResolvable<IMessageHandler<RequestQueryCommand>>
    {
        private readonly IIntegrationContext _context;

        public RequestQueryCommandHandler(IIntegrationContext context)
        {
            _context = context;
        }

        public Task Handle(RequestQueryCommand message, CancellationToken token)
        {
            var query = new Query(message.Id);

            return _context.Request<Query, Reply>(query, token);
        }
    }
}