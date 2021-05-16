namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System.Linq;
    using Abstractions;
    using ValueObjects;

    internal class ChangeParameterVisitor : IntermediateExpressionVisitorBase
    {
        private readonly ParameterExpression _parameter;

        public ChangeParameterVisitor(ParameterExpression parameter)
        {
            _parameter = parameter;
        }

        protected override ParameterExpression VisitParameter(ParameterExpression parameterExpression)
        {
            return _parameter;
        }

        protected override IIntermediateExpression VisitNamedSource(NamedSourceExpression namedSourceExpression)
        {
            return new NamedSourceExpression(
                namedSourceExpression.ItemType,
                namedSourceExpression.Source,
                VisitParameter(namedSourceExpression.Parameter));
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