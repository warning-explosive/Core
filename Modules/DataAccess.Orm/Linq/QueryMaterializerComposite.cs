namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Transaction;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;

    [Component(EnLifestyle.Singleton)]
    internal class QueryMaterializerComposite : IQueryMaterializerComposite,
                                                IResolvable<IQueryMaterializerComposite>
    {
        private readonly IReadOnlyDictionary<Type, IQueryMaterializer> _map;

        public QueryMaterializerComposite(IEnumerable<IQueryMaterializer> materializers)
        {
            _map = materializers.ToDictionary(static translator => translator.GetType().ExtractGenericArgumentAt(typeof(IQueryMaterializer<>)));
        }

        public Task<object?> MaterializeScalar(
            IAdvancedDatabaseTransaction transaction,
            IQuery query,
            Type type,
            CancellationToken token)
        {
            return _map.TryGetValue(query.GetType(), out var materializer)
                ? materializer.MaterializeScalar(transaction, query, type, token)
                : throw new NotSupportedException($"Unsupported query type {query.GetType()}");
        }

        public IAsyncEnumerable<object?> Materialize(
            IAdvancedDatabaseTransaction transaction,
            IQuery query,
            Type type,
            CancellationToken token)
        {
            return _map.TryGetValue(query.GetType(), out var materializer)
                ? materializer.Materialize(transaction, query, type, token)
                : throw new NotSupportedException($"Unsupported query type {query.GetType()}");
        }
    }
}