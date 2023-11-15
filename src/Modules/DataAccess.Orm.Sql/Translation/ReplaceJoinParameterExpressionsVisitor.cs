namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using Expressions;

    internal class ReplaceJoinParameterExpressionsVisitor : SqlExpressionVisitorBase
    {
        private readonly IReadOnlyDictionary<Type, ParameterExpression> _replacements;

        private ReplaceJoinParameterExpressionsVisitor(JoinExpression joinExpression)
        {
            _replacements ??= ExtractParametersVisitor.ExtractParameters(joinExpression);
        }

        public static ISqlExpression Replace(ISqlExpression expression, JoinExpression joinExpression)
        {
            return new ReplaceJoinParameterExpressionsVisitor(joinExpression).Visit(expression);
        }

        protected override ISqlExpression VisitColumnExpression(ColumnExpression columnExpression)
        {
            if (_replacements.TryGetValue(columnExpression.Type, out var parameterExpression))
            {
                return parameterExpression;
            }

            return base.VisitColumnExpression(columnExpression);
        }
    }
}