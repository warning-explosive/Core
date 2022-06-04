namespace SpaceEngineers.Core.GenericHost.Test.Mocks
{
    using System.Collections.Concurrent;
    using System.Linq.Expressions;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;

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