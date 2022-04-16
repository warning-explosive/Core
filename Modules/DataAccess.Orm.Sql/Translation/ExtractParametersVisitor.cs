namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Collections.Generic;
    using Expressions;

    internal class ExtractParametersVisitor : IntermediateExpressionVisitorBase
    {
        private const int BindingParameterOffset = 1_000_000;

        private int _currentIndex;

        public ExtractParametersVisitor()
        {
            Parameters = new Dictionary<int, ParameterExpression>();
            _currentIndex = 0;
        }

        public Dictionary<int, ParameterExpression> Parameters { get; }

        protected override IIntermediateExpression VisitParameter(ParameterExpression parameterExpression)
        {
            Parameters.Add(_currentIndex++, parameterExpression);
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
                Parameters.Add(BindingParameterOffset + _currentIndex++, parameterExpression);
                return simpleBindingExpression;
            }

            return base.VisitSimpleBinding(simpleBindingExpression);
        }
    }
}