namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Basics;
    using Expressions;
    using Extensions;

    internal class ReplaceJoinBindingsVisitor : IntermediateExpressionVisitorBase
    {
        private readonly IReadOnlyDictionary<Type, IIntermediateExpression> _replacements;
        private readonly bool _applyNaming;

        public ReplaceJoinBindingsVisitor(JoinExpression joinExpression, bool applyNaming)
        {
            _replacements = joinExpression
                .ExtractParameters()
                .GroupBy(parameter => parameter.Value.Type)
                .ToDictionary(
                    grp => grp.Key,
                    grp => grp
                        .OrderBy(parameter => parameter.Key)
                        .Select(parameter => parameter.Value)
                        .Cast<IIntermediateExpression>()
                        .First());

            _applyNaming = applyNaming;
        }

        protected override IIntermediateExpression VisitSimpleBinding(SimpleBindingExpression simpleBindingExpression)
        {
            var stack = new Stack<SimpleBindingExpression>();

            foreach (var expression in simpleBindingExpression.FlattenCompletely())
            {
                if (expression is not SimpleBindingExpression bindingExpression)
                {
                    break;
                }

                if (!_replacements.TryGetValue(bindingExpression.Type, out var replacement))
                {
                    stack.Push(bindingExpression);
                    continue;
                }

                if (!stack.Any())
                {
                    return replacement;
                }

                replacement = stack.Aggregate(
                    replacement,
                    (acc, next) => new SimpleBindingExpression(next.Member, next.Type, acc));

                stack.Push(bindingExpression);

                if (!_applyNaming)
                {
                    return replacement;
                }

                var name = stack.Select(binding => binding.Member.Name).ToString("_");

                return new NamedBindingExpression(name, replacement);
            }

            return base.VisitSimpleBinding(simpleBindingExpression);
        }

        protected override IIntermediateExpression VisitNamedBinding(NamedBindingExpression namedBindingExpression)
        {
            return new NamedBindingExpression(
                namedBindingExpression.Name,
                NamedBindingExpression.Unwrap(Visit(namedBindingExpression.Source)));
        }
    }
}