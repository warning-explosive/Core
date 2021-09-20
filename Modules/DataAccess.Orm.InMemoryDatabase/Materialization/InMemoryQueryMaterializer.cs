namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Materialization
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
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