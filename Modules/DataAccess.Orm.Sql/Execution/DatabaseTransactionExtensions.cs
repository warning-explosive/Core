namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Api.Model;
    using Basics;
    using Model;
    using Orm.Transaction;

    /// <summary>
    /// DatabaseTransactionExtensions
    /// </summary>
    public static class DatabaseTransactionExtensions
    {
        /// <summary>
        /// Creates entry point for every linq query with mtm entity
        /// </summary>
        /// <param name="context">IDatabaseContext</param>
        /// <param name="modelProvider">modelProvider</param>
        /// <param name="columnAccessor">Column accessor</param>
        /// <typeparam name="TLeft">TLeft type-argument</typeparam>
        /// <typeparam name="TRight">TRight type-argument</typeparam>
        /// <typeparam name="TLeftKey">TLeftKey type-argument</typeparam>
        /// <typeparam name="TRightKey">TRightKey type-argument</typeparam>
        /// <returns>Linq query</returns>
        public static IQueryable<BaseMtmDatabaseEntity<TLeftKey, TRightKey>> AllMtm<TLeft, TRight, TLeftKey, TRightKey>(
            this IDatabaseContext context,
            IModelProvider modelProvider,
            Expression<Func<TLeft, IEnumerable<TRight>>> columnAccessor)
            where TLeft : IDatabaseEntity<TLeftKey>
            where TRight : IDatabaseEntity<TRightKey>
            where TLeftKey : notnull
            where TRightKey : notnull
        {
            var columnName = ExtractMemberAccessExpressionVisitor.ExtractName(columnAccessor);

            var table = modelProvider.Tables[typeof(TLeft)];

            var mtmType = table.Columns[$"{columnName}_{nameof(BaseMtmDatabaseEntity<TLeftKey, TRightKey>.Left)}"].MultipleRelationTable!;

            return context
                .CallMethod(nameof(IDatabaseContext.All))
                .WithTypeArgument(mtmType)
                .Invoke<IQueryable<BaseMtmDatabaseEntity<TLeftKey, TRightKey>>>();
        }

        private class ExtractMemberAccessExpressionVisitor : ExpressionVisitor
        {
            private MemberInfo? _member;

            private ExtractMemberAccessExpressionVisitor()
            {
            }

            public static string ExtractName(Expression expression)
            {
                var visitor = new ExtractMemberAccessExpressionVisitor();

                _ = visitor.Visit(expression);

                return visitor
                    ._member
                    .EnsureNotNull($"Unable to find member name in expression: {expression}")
                    .Name;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (_member == null)
                {
                    _member = node.Member;
                }
                else
                {
                    throw new InvalidOperationException("Mtm member accessor should have only one member in the chain");
                }

                return base.VisitMember(node);
            }
        }
    }
}