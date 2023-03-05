﻿namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Collections.Generic;
    using Expressions;

    internal class ExtractParametersVisitor : SqlExpressionVisitorBase
    {
        private const int Offset = 1_000_000;

        private int _currentIndex;

        public ExtractParametersVisitor()
        {
            Parameters = new Dictionary<int, ParameterExpression>();
            _currentIndex = 0;
        }

        public Dictionary<int, ParameterExpression> Parameters { get; }

        protected override ISqlExpression VisitParameter(ParameterExpression parameterExpression)
        {
            Parameters.Add(_currentIndex++, parameterExpression);
            return parameterExpression;
        }

        protected override ISqlExpression VisitNamedSource(NamedSourceExpression namedSourceExpression)
        {
            var parameter = Visit(namedSourceExpression.Parameter);
            var source = Visit(namedSourceExpression.Source);

            return new NamedSourceExpression(
                namedSourceExpression.Type,
                source,
                parameter);
        }

        protected override ISqlExpression VisitColumnExpression(ColumnExpression columnExpression)
        {
            if (columnExpression.Source is ParameterExpression parameterExpression)
            {
                Parameters.Add(Offset + _currentIndex++, parameterExpression);
                return columnExpression;
            }

            return base.VisitColumnExpression(columnExpression);
        }
    }
}