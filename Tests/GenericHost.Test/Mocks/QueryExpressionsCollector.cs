namespace SpaceEngineers.Core.GenericHost.Test.Mocks
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq.Expressions;
    using DataAccess.Orm.Sql.Linq;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;

    [ManuallyRegisteredComponent(nameof(TranslationTest))]
    internal class QueryExpressionsCollector : IResolvable<QueryExpressionsCollector>,
                                               IDisposable
    {
        private readonly IAsyncQueryProvider _queryProvider;
        private readonly EventHandler<ExecutedExpressionEventArgs> _subscription;

        public QueryExpressionsCollector(IAsyncQueryProvider queryProvider)
        {
            _queryProvider = queryProvider;
            _subscription = Collect;

            _queryProvider.ExpressionExecuted += _subscription;

            Expressions = new ConcurrentQueue<Expression>();
        }

        public ConcurrentQueue<Expression> Expressions { get; }

        public void Dispose()
        {
            _queryProvider.ExpressionExecuted -= _subscription;
        }

        private void Collect(object? sender, ExecutedExpressionEventArgs args)
        {
            Expressions.Enqueue(args.Expression);
        }
    }
}