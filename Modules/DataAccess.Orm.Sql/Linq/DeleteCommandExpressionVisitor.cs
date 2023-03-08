namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Linq
{
    using System;
    using System.Linq.Expressions;
    using Basics;
    using Orm.Linq;
    using Orm.Transaction;

    internal class DeleteCommandExpressionVisitor<TEntity> : ExpressionVisitor
    {
        private IAdvancedDatabaseTransaction? _transaction;
        private Expression<Func<TEntity, bool>>? _predicate;
        private string? _cacheKey;

        private DeleteCommandExpressionVisitor()
        {
        }

        public static (IAdvancedDatabaseTransaction, Expression<Func<TEntity, bool>>, string) Extract(Expression expression)
        {
            var visitor = new DeleteCommandExpressionVisitor<TEntity>();

            _ = visitor.Visit(expression);

            if (visitor._transaction == null
                || visitor._predicate == null)
            {
                throw new InvalidOperationException("Unable to determine delete query root");
            }

            if (visitor._cacheKey == null)
            {
                throw new InvalidOperationException("Unable to determine cached expression extension");
            }

            return (visitor._transaction, visitor._predicate, visitor._cacheKey);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var method = node.Method.GenericMethodDefinitionOrSelf();

            if (method == LinqMethods.RepositoryDelete())
            {
                _transaction = (IAdvancedDatabaseTransaction)((ConstantExpression)node.Arguments[0]).Value;
            }

            if (method == LinqMethods.RepositoryDeleteWhere())
            {
                _predicate = (Expression<Func<TEntity, bool>>)node.Arguments[1].UnwrapUnaryExpression();
            }

            if (method == LinqMethods.CachedDeleteExpression())
            {
                _cacheKey = (string)((ConstantExpression)node.Arguments[1]).Value;
            }

            return base.VisitMethodCall(node);
        }
    }
}