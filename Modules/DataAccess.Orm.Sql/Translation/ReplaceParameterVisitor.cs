namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using Expressions;

    internal class ReplaceParameterVisitor : SqlExpressionVisitorBase
    {
        private readonly ParameterExpression _parameterExpression;

        public ReplaceParameterVisitor(ParameterExpression parameterExpression)
        {
            _parameterExpression = parameterExpression;
        }

        protected override ISqlExpression VisitParameter(ParameterExpression parameterExpression)
        {
            return _parameterExpression;
        }
    }
}