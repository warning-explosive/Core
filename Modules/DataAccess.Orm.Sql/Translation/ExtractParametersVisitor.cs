namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Basics;
    using Expressions;

    internal class ExtractParametersVisitor : SqlExpressionVisitorBase
    {
        private readonly Dictionary<Type, ParameterExpression> _parameters;

        private ExtractParametersVisitor()
        {
            _parameters = new Dictionary<Type, ParameterExpression>();
        }

        public static IReadOnlyDictionary<Type, ParameterExpression> ExtractParameters(ISqlExpression expression)
        {
            var extractor = new ExtractParametersVisitor();

            _ = extractor.Visit(expression);

            return extractor._parameters;
        }

        public static ParameterExpression ExtractParameter(
            ISqlExpression expression,
            Type parameterType)
        {
            var extractor = new ExtractParametersVisitor();

            _ = extractor.Visit(expression);

            return extractor._parameters.TryGetValue(parameterType, out var parameter)
                ? parameter
                : throw new InvalidOperationException($"Unable to find parameter expression with type {parameterType}");
        }

        public static bool TryExtractParameter(
            ISqlExpression expression,
            Type parameterType,
            [NotNullWhen(true)] out ParameterExpression? parameter)
        {
            var extractor = new ExtractParametersVisitor();

            _ = extractor.Visit(expression);

            parameter = null;

            return extractor._parameters.TryGetValue(parameterType, out parameter);
        }

        protected override ISqlExpression VisitParameter(ParameterExpression parameterExpression)
        {
            _parameters.GetOrAdd(parameterExpression.Type, parameterExpression);

            return base.VisitParameter(parameterExpression);
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
    }
}