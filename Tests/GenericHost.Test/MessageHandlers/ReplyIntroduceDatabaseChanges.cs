namespace SpaceEngineers.Core.GenericHost.Test.MessageHandlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using DataAccess.Orm.Sql.Linq;
    using DataAccess.Orm.Transaction;
    using DatabaseEntities;
    using GenericEndpoint.Api.Abstractions;
    using Messages;

    [Component(EnLifestyle.Transient)]
    internal class ReplyIntroduceDatabaseChanges : IMessageHandler<Reply>,
                                                   IResolvable<IMessageHandler<Reply>>
    {
        private readonly IDatabaseContext _databaseContext;

        public ReplyIntroduceDatabaseChanges(IDatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public Task Handle(Reply message, CancellationToken token)
        {
            return _databaseContext
                .Insert(new[] { DatabaseEntity.Generate() }, EnInsertBehavior.Default)
                .Invoke(token);
        }
    }
}