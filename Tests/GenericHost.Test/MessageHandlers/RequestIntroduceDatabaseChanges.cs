namespace SpaceEngineers.Core.GenericHost.Test.MessageHandlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using DataAccess.Orm.Linq;
    using DataAccess.Orm.Sql.Linq;
    using DataAccess.Orm.Transaction;
    using DatabaseEntities;
    using GenericEndpoint.Api.Abstractions;
    using Messages;

    [Component(EnLifestyle.Transient)]
    internal class RequestIntroduceDatabaseChanges : IMessageHandler<Request>,
                                                     IResolvable<IMessageHandler<Request>>
    {
        private readonly IIntegrationContext _context;
        private readonly IDatabaseContext _databaseContext;

        public RequestIntroduceDatabaseChanges(
            IIntegrationContext context,
            IDatabaseContext databaseContext)
        {
            _context = context;
            _databaseContext = databaseContext;
        }

        public async Task Handle(Request message, CancellationToken token)
        {
            await _databaseContext
               .Insert(new[] { DatabaseEntity.Generate() }, EnInsertBehavior.Default)
               .CachedExpression("BD9DABE3-B49F-4D3E-ADCC-3C22B387AA14")
               .Invoke(token)
               .ConfigureAwait(false);

            await _context
               .Reply(message, new Reply(message.Id), token)
               .ConfigureAwait(false);
        }
    }
}