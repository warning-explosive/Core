namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Materialization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Exceptions;
    using Api.Transaction;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Orm.Linq;
    using Translation;

    [Component(EnLifestyle.Scoped)]
    internal class InMemoryQueryMaterializer<T> : IQueryMaterializer<InMemoryQuery, T>
    {
        private readonly IAdvancedDatabaseTransaction _transaction;

        public InMemoryQueryMaterializer(IAdvancedDatabaseTransaction transaction)
        {
            _transaction = transaction;
        }

        public async Task<T> MaterializeScalar(InMemoryQuery query, CancellationToken token)
        {
            var expression = new TranslationExpressionVisitor(_transaction)
                .Visit(query.Expression)
                .EnsureNotNull(() => new TranslationException(query.Expression));

            var scalar = Expression.Lambda<Func<T>>(expression).Compile().Invoke();

            return await Task.FromResult(scalar).ConfigureAwait(false);
        }

        public async IAsyncEnumerable<T> Materialize(InMemoryQuery query, [EnumeratorCancellation] CancellationToken token)
        {
            var expression = new TranslationExpressionVisitor(_transaction).Visit(query.Expression);

            var enumerableQuery = new EnumerableQuery<T>(expression);

            foreach (var item in enumerableQuery)
            {
                yield return await Task.FromResult(item).ConfigureAwait(false);
            }
        }
    }
}