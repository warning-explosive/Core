namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Linq;
    using System.Linq.Expressions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Attributes;
    using CompositionRoot;

    [Component(EnLifestyle.Singleton)]
    [After(typeof(BinaryExpressionVisitor))]
    internal class SubQueryExpressionVisitor : ExpressionVisitor,
                                               ILinqExpressionPreprocessor,
                                               ICollectionResolvable<ILinqExpressionPreprocessor>
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IQueryProvider _queryProvider;

        public SubQueryExpressionVisitor(
            IDependencyContainer dependencyContainer,
            IQueryProvider queryProvider)
        {
            _dependencyContainer = dependencyContainer;
            _queryProvider = queryProvider;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Value is IQueryable subQuery
                && typeof(IQueryable).IsAssignableFrom(subQuery.Expression.Type))
            {
                var expression = _dependencyContainer
                    .Resolve<ILinqExpressionPreprocessorComposite>()
                    .Visit(subQuery.Expression);

                return Expression.Constant(_queryProvider.CreateQuery(expression), node.Type);
            }

            return base.VisitConstant(node);
        }
    }
}