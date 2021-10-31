namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Collections.Generic;
    using Expressions;

    internal class ExtractParametersVisitor : IntermediateExpressionVisitorBase
    {
        private readonly List<ParameterExpression> _parameters;

        private ExtractParametersVisitor()
        {
            _parameters = new List<ParameterExpression>();
        }

        public static IReadOnlyCollection<ParameterExpression> ExtractParameters(IIntermediateExpression expression)
        {
            var extractor = new ExtractParametersVisitor();
            _ = extractor.Visit(expression);
            return extractor._parameters;
        }

        protected override IIntermediateExpression VisitParameter(ParameterExpression parameterExpression)
        {
            _parameters.Add(parameterExpression);
            return parameterExpression;
        }
    }
}