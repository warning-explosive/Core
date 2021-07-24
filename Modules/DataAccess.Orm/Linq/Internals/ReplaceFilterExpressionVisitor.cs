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
                .Select(binding => (INamedIntermediateExpression)binding)
                .ToDictionary(binding => binding.Name,
                    NamedBindingExpression.Unwrap,
                    StringComparer.OrdinalIgnoreCase);
        }

        protected override IIntermediateExpression VisitSimpleBinding(SimpleBindingExpression simpleBindingExpression)
        {
            return simpleBindingExpression.Expression is ParameterExpression
                   && _replacements.ContainsKey(simpleBindingExpression.Name)
                ? _replacements[simpleBindingExpression.Name]
                : base.VisitSimpleBinding(simpleBindingExpression);
        }
    }
}