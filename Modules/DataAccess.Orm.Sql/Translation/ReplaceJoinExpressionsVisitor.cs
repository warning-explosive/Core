namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Basics;
    using Expressions;

    internal class ReplaceJoinExpressionsVisitor : SqlExpressionVisitorBase
    {
        private readonly IReadOnlyDictionary<Type, ITypedSqlExpression> _replacements;
        private readonly bool _applyNaming;

        public ReplaceJoinExpressionsVisitor(JoinExpression joinExpression, bool applyNaming)
        {
            _replacements = joinExpression
                .ExtractParameters()
                .GroupBy(parameter => parameter.Value.Type)
                .ToDictionary(
                    grp => grp.Key,
                    grp => grp
                        .OrderBy(parameter => parameter.Key)
                        .Select(parameter => parameter.Value)
                        .Cast<ITypedSqlExpression>()
                        .First());

            _applyNaming = applyNaming;
        }

        protected override ISqlExpression VisitColumnExpression(ColumnExpression columnExpression)
        {
            var stack = new Stack<ColumnExpression>();

            foreach (var expression in columnExpression.FlattenCompletely())
            {
                if (expression is not ColumnExpression column)
                {
                    break;
                }

                if (!_replacements.TryGetValue(column.Type, out var replacement))
                {
                    stack.Push(column);
                    continue;
                }

                if (!stack.Any())
                {
                    return replacement;
                }

                replacement = stack.Aggregate(
                    replacement,
                    (acc, next) => new ColumnExpression(next.Member, next.Type, acc));

                stack.Push(column);

                if (!_applyNaming)
                {
                    return replacement;
                }

                var name = stack.Select(static columnExpression => columnExpression.Member.Name).ToString("_");

                return new RenameExpression(replacement.Type, name, replacement);
            }

            return base.VisitColumnExpression(columnExpression);
        }

        protected override ISqlExpression VisitRename(RenameExpression expression)
        {
            return new RenameExpression(
                expression.Type,
                expression.Name,
                RenameExpression.UnwrapRenames(Visit(expression.Source)));
        }
    }
}