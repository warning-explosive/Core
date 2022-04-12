namespace SpaceEngineers.Core.Modules.Test.Mocks
{
    using System.Collections.Concurrent;
    using System.Linq.Expressions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;

    [ManuallyRegisteredComponent(nameof(DataAccess))]
    internal class QueryExpressionsCollector : IResolvable<QueryExpressionsCollector>
    {
        public ConcurrentQueue<Expression> Expressions { get; } = new ConcurrentQueue<Expression>();

        public void Collect(Expression expression)
        {
            Expressions.Enqueue(expression);
        }
    }
}