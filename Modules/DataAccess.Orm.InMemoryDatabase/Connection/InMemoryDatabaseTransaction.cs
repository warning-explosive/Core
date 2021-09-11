namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Connection
{
    using System;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using GenericDomain.Api.Abstractions;

    [Component(EnLifestyle.Scoped)]
    internal class InMemoryDatabaseTransaction : IDatabaseTransaction, IDisposable
    {
        public bool HasChanges => throw new NotImplementedException("#131 - track domain entities");

        public Task Track(IAggregate aggregate, CancellationToken token)
        {
            throw new NotImplementedException("#131 - track domain entities");
        }

        public Task<IDbTransaction> Open(CancellationToken token)
        {
            throw new NotImplementedException("#112");
        }

        public Task Close(bool commit, CancellationToken token)
        {
            throw new NotImplementedException("#112");
        }

        public void Dispose()
        {
            Close(false, CancellationToken.None).Wait();
        }
    }
}