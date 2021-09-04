namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Internals
{
    using System.Linq.Expressions;

    internal class ExtractLambdaExpressionVisitor : ExpressionVisitor
    {
        private LambdaExpression? _lambdaExpression;

        public LambdaExpression? Extract(Expression nodeArgument)
        {
            _ = Visit(nodeArgument);

            return _lambdaExpression;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            _lambdaExpression = node;

            return base.VisitLambda(node);
        }
    }
}