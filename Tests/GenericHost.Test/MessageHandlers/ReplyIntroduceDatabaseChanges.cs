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
                .CachedExpression("63A280FE-CCCD-4C13-B357-4DD40EA345A6")
                .Invoke(token);
        }
    }
}