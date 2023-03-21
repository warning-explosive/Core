namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Transaction
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Basics;
    using Model;

    internal static class DatabaseTransactionExtensions
    {
        public static IQueryable AllMtm<TLeft, TRight, TLeftKey, TRightKey>(
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

            var mtmType = table.Columns[columnName].MultipleRelationTable!;

            return context
                .CallMethod(nameof(IDatabaseContext.All))
                .WithTypeArgument(mtmType)
                .Invoke<IQueryable>();
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

                return visitor._member?.Name
                       ?? throw new InvalidOperationException($"Unable to find member name in expression: {expression}");
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