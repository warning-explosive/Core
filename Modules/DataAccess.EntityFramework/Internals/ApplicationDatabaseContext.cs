namespace SpaceEngineers.Core.DataAccess.EntityFramework.Internals
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;
    using Contract.Abstractions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;

    [Component(EnLifestyle.Scoped)]
    internal class ApplicationDatabaseContext : DbContext,
                                                IDatabaseTransactionProvider
    {
        private readonly IEnumerable<IDatabaseModelBuilder> _databaseModelBuilders;
        
        private IDbContextTransaction? _transaction;

        public ApplicationDatabaseContext(DbContextOptions<ApplicationDatabaseContext> options,
                                          IEnumerable<IDatabaseModelBuilder> databaseModelBuilders)
            : base(options)
        {
            _databaseModelBuilders = databaseModelBuilders;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _databaseModelBuilders.Each(it => it.Build(modelBuilder));
        }
        
        public async Task OpenTransaction(CancellationToken token)
        {
            _transaction = await Database.BeginTransactionAsync(token).ConfigureAwait(false);
        }

        public Task Commit(CancellationToken token)
        {
            return _transaction.CommitAsync(token);
        }

        public Task Rollback(CancellationToken token)
        {
            return _transaction.RollbackAsync(token);
        }
    }
}