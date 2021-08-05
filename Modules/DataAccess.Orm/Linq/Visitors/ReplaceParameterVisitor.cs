namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Visitors
{
    using Abstractions;
    using Expressions;

    internal class ReplaceParameterVisitor : IntermediateExpressionVisitorBase
    {
        private readonly ParameterExpression _parameterExpression;

        public ReplaceParameterVisitor(ParameterExpression parameterExpression)
        {
            _parameterExpression = parameterExpression;
        }

        protected override IIntermediateExpression VisitParameter(ParameterExpression parameterExpression)
        {
            return _parameterExpression;
        }
    }
}