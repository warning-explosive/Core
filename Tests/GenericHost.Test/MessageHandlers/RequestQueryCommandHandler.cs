namespace SpaceEngineers.Core.GenericHost.Test.MessageHandlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Messages;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.GenericEndpoint.Api.Abstractions;

    [Component(EnLifestyle.Transient)]
    internal class RequestQueryCommandHandler : IMessageHandler<RequestQueryCommand>,
                                                IResolvable<IMessageHandler<RequestQueryCommand>>
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