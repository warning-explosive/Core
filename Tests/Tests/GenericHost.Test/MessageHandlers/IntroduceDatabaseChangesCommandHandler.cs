namespace SpaceEngineers.Core.GenericHost.Test.MessageHandlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using DataAccess.Orm.Sql.Linq;
    using DataAccess.Orm.Sql.Transaction;
    using DatabaseEntities;
    using GenericEndpoint.Api.Abstractions;
    using Messages;

    [Component(EnLifestyle.Transient)]
    internal class IntroduceDatabaseChangesCommandHandler : IMessageHandler<Command>,
                                                            IResolvable<IMessageHandler<Command>>
    {
        private readonly IDatabaseContext _databaseContext;

        public IntroduceDatabaseChangesCommandHandler(IDatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public Task Handle(Command message, CancellationToken token)
        {
            return _databaseContext
                .Insert(new[] { DatabaseEntity.Generate() }, EnInsertBehavior.Default)
                .CachedExpression("8981C2BE-2FE9-4932-841B-94A31C3DE136")
                .Invoke(token);
        }
    }
}