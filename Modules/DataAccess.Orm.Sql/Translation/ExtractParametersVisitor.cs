namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Collections.Generic;
    using Expressions;

    internal class ExtractParametersVisitor : IntermediateExpressionVisitorBase
    {
        private const int BindingParameterOffset = 1_000_000;

        private readonly Dictionary<int, ParameterExpression> _parameters;
        private int _currentIndex;

        private ExtractParametersVisitor()
        {
            _parameters = new Dictionary<int, ParameterExpression>();
            _currentIndex = 0;
        }

        public static IReadOnlyDictionary<int, ParameterExpression> ExtractParameters(IIntermediateExpression expression)
        {
            var extractor = new ExtractParametersVisitor();
            _ = extractor.Visit(expression);
            return extractor._parameters;
        }

        protected override IIntermediateExpression VisitParameter(ParameterExpression parameterExpression)
        {
            _parameters.Add(_currentIndex++, parameterExpression);
            return parameterExpression;
        }

        protected override IIntermediateExpression VisitNamedSource(NamedSourceExpression namedSourceExpression)
        {
            var parameter = Visit(namedSourceExpression.Parameter);
            var source = Visit(namedSourceExpression.Source);

            return new NamedSourceExpression(
                namedSourceExpression.Type,
                source,
                parameter);
        }

        protected override IIntermediateExpression VisitSimpleBinding(SimpleBindingExpression simpleBindingExpression)
        {
            if (simpleBindingExpression.Source is ParameterExpression parameterExpression)
            {
                _parameters.Add(BindingParameterOffset + _currentIndex++, parameterExpression);
                return simpleBindingExpression;
            }

            return base.VisitSimpleBinding(simpleBindingExpression);
        }
    }
}