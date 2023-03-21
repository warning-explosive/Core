namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Linq.Expressions;
    using Basics;
    using Linq;

    internal class ExtractUpdateQueryRootExpressionVisitor : ExpressionVisitor
    {
        private bool _isUpdateQuery;

        private ExtractUpdateQueryRootExpressionVisitor()
        {
        }

        public static bool IsUpdateQuery(Expression expression)
        {
            var visitor = new ExtractUpdateQueryRootExpressionVisitor();
            _ = visitor.Visit(expression);
            return visitor._isUpdateQuery;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var method = node.Method.GenericMethodDefinitionOrSelf();

            if (method == LinqMethods.RepositoryUpdate())
            {
                _isUpdateQuery = true;
            }

            return base.VisitMethodCall(node);
        }
    }
}