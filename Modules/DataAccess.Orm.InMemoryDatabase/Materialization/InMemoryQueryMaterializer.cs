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
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Database;
    using Linq;
    using Translation;

    [Component(EnLifestyle.Scoped)]
    internal class InMemoryQueryMaterializer<T> : IQueryMaterializer<InMemoryQuery, T>
    {
        private readonly IInMemoryDatabase _database;

        public InMemoryQueryMaterializer(IInMemoryDatabase database)
        {
            _database = database;
        }

        public async Task<T> MaterializeScalar(InMemoryQuery query, CancellationToken token)
        {
            var expression = new TranslationExpressionVisitor(_database)
                .Visit(query.Expression)
                .EnsureNotNull(() => new TranslationException(query.Expression));

            var scalar = Expression.Lambda<Func<T>>(expression).Compile().Invoke();

            return await Task.FromResult(scalar).ConfigureAwait(false);
        }

        public async IAsyncEnumerable<T> Materialize(InMemoryQuery query, [EnumeratorCancellation] CancellationToken token)
        {
            var expression = new TranslationExpressionVisitor(_database).Visit(query.Expression);

            var enumerableQuery = new EnumerableQuery<T>(expression);

            foreach (var item in enumerableQuery)
            {
                yield return await Task.FromResult(item).ConfigureAwait(false);
            }
        }
    }
}