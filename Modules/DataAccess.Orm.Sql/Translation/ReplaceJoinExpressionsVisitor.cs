namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Basics;
    using Expressions;

    internal class ReplaceJoinExpressionsVisitor : SqlExpressionVisitorBase
    {
        private readonly JoinExpression _joinExpression;
        private readonly bool _applyNaming;

        private IReadOnlyDictionary<Type, ParameterExpression>? _replacements;

        private ReplaceJoinExpressionsVisitor(JoinExpression joinExpression, bool applyNaming)
        {
            _joinExpression = joinExpression;
            _applyNaming = applyNaming;
        }

        public static ISqlExpression Replace(ISqlExpression expression, JoinExpression joinExpression, bool applyNaming)
        {
            return new ReplaceJoinExpressionsVisitor(joinExpression, applyNaming).Visit(expression);
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

                _replacements ??= ExtractParametersVisitor.ExtractParameters(_joinExpression);

                if (!_replacements.TryGetValue(column.Type, out var parameterExpression)
                    || parameterExpression is not ITypedSqlExpression replacement)
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