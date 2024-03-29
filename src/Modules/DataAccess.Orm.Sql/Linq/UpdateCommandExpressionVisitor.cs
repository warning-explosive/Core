namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Basics;
    using Transaction;

    internal class UpdateCommandExpressionVisitor<TEntity> : ExpressionVisitor
    {
        private IAdvancedDatabaseTransaction? _transaction;
        private List<Expression<Action<TEntity>>> _setExpressions;
        private Expression<Func<TEntity, bool>>? _predicate;
        private string? _cacheKey;

        private UpdateCommandExpressionVisitor()
        {
            _setExpressions = new List<Expression<Action<TEntity>>>();
        }

        public static (IAdvancedDatabaseTransaction, IReadOnlyCollection<Expression<Action<TEntity>>>, Expression<Func<TEntity, bool>>, string) Extract(Expression expression)
        {
            var visitor = new UpdateCommandExpressionVisitor<TEntity>();

            _ = visitor.Visit(expression);

            if (visitor._transaction == null
                || !visitor._setExpressions.Any()
                || visitor._predicate == null)
            {
                throw new InvalidOperationException("Unable to determine update query root");
            }

            if (visitor._cacheKey == null)
            {
                throw new InvalidOperationException("Unable to determine cached expression extension");
            }

            return (visitor._transaction, visitor._setExpressions, visitor._predicate, visitor._cacheKey);
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

            if (method == LinqMethods.CachedUpdateExpression())
            {
                _cacheKey = (string)((ConstantExpression)node.Arguments[1]).Value;
            }

            return base.VisitMethodCall(node);
        }
    }
}