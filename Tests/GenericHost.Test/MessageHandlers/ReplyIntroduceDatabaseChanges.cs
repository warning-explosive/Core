namespace SpaceEngineers.Core.GenericHost.Test.MessageHandlers
{
    using System;
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
               .Write<DatabaseEntity, Guid>()
               .Insert(new[] { DatabaseEntity.Generate() }, EnInsertBehavior.Default, token);
        }
    }
}