namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using Expressions;

    internal class ReplaceFilterExpressionVisitor : IntermediateExpressionVisitorBase
    {
        private readonly IReadOnlyDictionary<string, IIntermediateExpression> _replacements;

        public ReplaceFilterExpressionVisitor(ProjectionExpression projection)
        {
            _replacements = projection
                .Bindings
                .OfType<INamedIntermediateExpression>()
                .ToDictionary(binding => binding.Name,
                    NamedBindingExpression.Unwrap,
                    StringComparer.OrdinalIgnoreCase);
        }

        protected override IIntermediateExpression VisitSimpleBinding(SimpleBindingExpression simpleBindingExpression)
        {
            return simpleBindingExpression.Expression is ParameterExpression
                   && _replacements.TryGetValue(simpleBindingExpression.Name, out var replacement)
                   && replacement.Type == simpleBindingExpression.Type
                ? replacement
                : base.VisitSimpleBinding(simpleBindingExpression);
        }

        protected override IIntermediateExpression VisitParameter(ParameterExpression parameterExpression)
        {
            return _replacements.Count == 1
                   && _replacements.Single().Value is { } binding
                   && binding.Type == parameterExpression.Type
                ? binding
                : parameterExpression;
        }
    }
}