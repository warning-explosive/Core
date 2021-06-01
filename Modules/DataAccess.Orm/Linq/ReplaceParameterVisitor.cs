namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System.Linq;
    using Abstractions;
    using ValueObjects;

    internal class ReplaceParameterVisitor : IntermediateExpressionVisitorBase
    {
        private readonly IIntermediateExpression _expression;

        public ReplaceParameterVisitor(IIntermediateExpression expression)
        {
            _expression = expression;
        }

        protected override IIntermediateExpression VisitParameter(ParameterExpression parameterExpression)
        {
            return _expression;
        }

        protected override IIntermediateExpression VisitNamedSource(NamedSourceExpression namedSourceExpression)
        {
            return new NamedSourceExpression(
                namedSourceExpression.ItemType,
                namedSourceExpression.Source,
                Visit(namedSourceExpression.Parameter));
        }

        protected override IIntermediateExpression VisitProjection(ProjectionExpression projectionExpression)
        {
            return new ProjectionExpression(
                projectionExpression.ItemType,
                projectionExpression.Source is NamedSourceExpression
                    ? Visit(projectionExpression.Source)
                    : projectionExpression.Source,
                projectionExpression.Bindings.Select(Visit));
        }
    }
}