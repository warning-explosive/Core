namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Basics;
    using CompositionRoot;
    using Model;
    using Transaction;

    internal class InsertCommandExpressionVisitor : ExpressionVisitor
    {
        private IDependencyContainer? _dependencyContainer;
        private IAdvancedDatabaseTransaction? _transaction;
        private IReadOnlyCollection<IDatabaseEntity>? _entities;
        private EnInsertBehavior? _insertBehavior;
        private string? _cacheKey;

        private InsertCommandExpressionVisitor()
        {
        }

        public static (IDependencyContainer, IAdvancedDatabaseTransaction, IReadOnlyCollection<IDatabaseEntity>, EnInsertBehavior, string) Extract(Expression expression)
        {
            var visitor = new InsertCommandExpressionVisitor();

            _ = visitor.Visit(expression);

            if (visitor._dependencyContainer == null
                || visitor._transaction == null
                || visitor._entities == null
                || visitor._insertBehavior == null)
            {
                throw new InvalidOperationException("Unable to determine insert query root");
            }

            if (visitor._cacheKey == null)
            {
                throw new InvalidOperationException("Unable to determine cached expression extension");
            }

            return (visitor._dependencyContainer, visitor._transaction, visitor._entities, visitor._insertBehavior.Value, visitor._cacheKey);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var method = node.Method.GenericMethodDefinitionOrSelf();

            if (method == LinqMethods.RepositoryInsert())
            {
                _transaction = (IAdvancedDatabaseTransaction)((ConstantExpression)node.Arguments[0]).Value;
                _entities = (IReadOnlyCollection<IDatabaseEntity>)((ConstantExpression)node.Arguments[1]).Value;
                _insertBehavior = (EnInsertBehavior)((ConstantExpression)node.Arguments[2]).Value;
            }

            if (method == LinqMethods.WithDependencyContainer())
            {
                _dependencyContainer = (IDependencyContainer)((ConstantExpression)node.Arguments[1]).Value;
            }

            if (method == LinqMethods.CachedInsertExpression())
            {
                _cacheKey = (string)((ConstantExpression)node.Arguments[1]).Value;
            }

            return base.VisitMethodCall(node);
        }
    }
}