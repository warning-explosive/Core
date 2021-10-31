namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Expressions;

    internal class ReplaceJoinBindingsVisitor : IntermediateExpressionVisitorBase
    {
        private readonly IReadOnlyDictionary<Type, ParameterExpression> _replacements;

        public ReplaceJoinBindingsVisitor(JoinExpression join)
        {
            _replacements = ExtractParametersVisitor
                .ExtractParameters(join)
                .Distinct()
                .ToDictionary(parameter => parameter.Type);
        }

        protected override IIntermediateExpression VisitSimpleBinding(SimpleBindingExpression simpleBindingExpression)
        {
            return _replacements.TryGetValue(simpleBindingExpression.Type, out var replacement)
                ? replacement
                : base.VisitSimpleBinding(simpleBindingExpression);
        }
    }
}