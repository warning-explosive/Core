namespace SpaceEngineers.Core.GenericHost.Test.MessageHandlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using DataAccess.Api.Persisting;
    using DataAccess.Api.Transaction;
    using DatabaseEntities;
    using GenericEndpoint.Api.Abstractions;
    using Messages;

    [Component(EnLifestyle.Transient)]
    internal class CommandIntroduceDatabaseChanges : IMessageHandler<Command>,
                                                     IResolvable<IMessageHandler<Command>>
    {
        private readonly IDatabaseContext _databaseContext;

        public CommandIntroduceDatabaseChanges(IDatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public Task Handle(Command message, CancellationToken token)
        {
            return _databaseContext
               .Write<DatabaseEntity>()
               .Insert(new[] { DatabaseEntity.Generate() }, EnInsertBehavior.Default, token);
        }
    }
}