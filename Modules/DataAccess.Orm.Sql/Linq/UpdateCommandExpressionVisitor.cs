namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Basics;
    using Orm.Linq;
    using Orm.Transaction;

    internal class UpdateCommandExpressionVisitor<TEntity> : ExpressionVisitor
    {
        private IAdvancedDatabaseTransaction? _transaction;
        private List<Expression<Action<TEntity>>> _setExpressions;
        private Expression<Func<TEntity, bool>>? _predicate;

        private UpdateCommandExpressionVisitor()
        {
            _setExpressions = new List<Expression<Action<TEntity>>>();
        }

        public static (IAdvancedDatabaseTransaction, IReadOnlyCollection<Expression<Action<TEntity>>>, Expression<Func<TEntity, bool>>) Extract(Expression expression)
        {
            var visitor = new UpdateCommandExpressionVisitor<TEntity>();

            _ = visitor.Visit(expression);

            if (visitor._transaction == null
                || !visitor._setExpressions.Any()
                || visitor._predicate == null)
            {
                throw new InvalidOperationException("Unable to determine update query root");
            }

            return (visitor._transaction, visitor._setExpressions, visitor._predicate);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var method = node.Method.GenericMethodDefinitionOrSelf();

            if (method == LinqMethods.RepositoryUpdate())
            {
                _transaction = (IAdvancedDatabaseTransaction)((ConstantExpression)node.Arguments[0]).Value;
            }

            if (method == LinqMethods.RepositoryUpdateWhere())
            {
                _predicate = (Expression<Func<TEntity, bool>>)node.Arguments[1].UnwrapUnaryExpression();
            }

            if (method == LinqMethods.RepositoryUpdateSet()
                || method == LinqMethods.RepositoryChainedUpdateSet())
            {
                _setExpressions.Add((Expression<Action<TEntity>>)node.Arguments[1].UnwrapUnaryExpression());
            }

            return base.VisitMethodCall(node);
        }
    }
}