namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Expressions;

    internal class ReplaceProjectionExpressionVisitor : SqlExpressionVisitorBase
    {
        private readonly IReadOnlyDictionary<string, ITypedSqlExpression>? _replacements;

        private ReplaceProjectionExpressionVisitor(ProjectionExpression projection)
        {
            _replacements = projection
                .Expressions
                .ToDictionary(
                    expression => (expression as ColumnExpression)?.Name ?? (expression as RenameExpression).Name,
                    expression => (ITypedSqlExpression)UnwrapRenames(expression),
                    StringComparer.OrdinalIgnoreCase);
        }

        public static ISqlExpression Compact(ISqlExpression expression, ProjectionExpression projection)
        {
            return new ReplaceProjectionExpressionVisitor(projection).Visit(expression);
        }

        protected override ISqlExpression VisitColumnExpression(ColumnExpression columnExpression)
        {
            if (columnExpression.Source is ParameterExpression
                && _replacements.TryGetValue(columnExpression.Name, out var projection)
                && projection.Type == columnExpression.Type)
            {
                return projection;
            }

            return base.VisitColumnExpression(columnExpression);
        }

        protected override ISqlExpression VisitParameter(ParameterExpression parameterExpression)
        {
            if (_replacements.Count == 1
                && _replacements.Single().Value is { } projection
                && projection.Type == parameterExpression.Type)
            {
                return projection;
            }

            return parameterExpression;
        }

        private static ISqlExpression UnwrapRenames(ISqlExpression expression)
        {
            while (true)
            {
                if (expression is not RenameExpression renameExpression)
                {
                    return expression;
                }

                expression = renameExpression.Source;
            }
        }
    }
}