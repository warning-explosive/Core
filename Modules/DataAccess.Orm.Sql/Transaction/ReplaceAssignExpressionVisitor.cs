namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Transaction
{
    using System;
    using System.Linq.Expressions;
    using Basics;
    using Orm.Linq;

    internal class ReplaceAssignExpressionVisitor : ExpressionVisitor
    {
        private ReplaceAssignExpressionVisitor()
        {
        }

        public static Expression<Action<TEntity>> Replace<TEntity>(Expression<Action<TEntity>> expression)
        {
            return (Expression<Action<TEntity>>)new ReplaceAssignExpressionVisitor().Visit(expression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var method = node.Method.GenericMethodDefinitionOrSelf();

            return method == LinqMethods.Assign()
                ? Expression.Assign(node.Arguments[0], node.Arguments[1])
                : base.VisitMethodCall(node);
        }
    }
}