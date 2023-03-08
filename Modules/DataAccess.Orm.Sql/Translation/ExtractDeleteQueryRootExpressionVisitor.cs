namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Linq.Expressions;
    using Basics;
    using Orm.Linq;

    internal class ExtractDeleteQueryRootExpressionVisitor : ExpressionVisitor
    {
        private bool _isDeleteQuery;

        private ExtractDeleteQueryRootExpressionVisitor()
        {
        }

        public static bool IsDeleteQuery(Expression expression)
        {
            var visitor = new ExtractDeleteQueryRootExpressionVisitor();
            _ = visitor.Visit(expression);
            return visitor._isDeleteQuery;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var method = node.Method.GenericMethodDefinitionOrSelf();

            if (method == LinqMethods.RepositoryDelete())
            {
                _isDeleteQuery = true;
            }

            return base.VisitMethodCall(node);
        }
    }
}