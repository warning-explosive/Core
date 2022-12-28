namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Basics;
    using Basics.Primitives;
    using Expressions;

    internal class CompactExpressionVisitor : SqlExpressionVisitorBase
    {
        private readonly IReadOnlyDictionary<string, ISqlExpression> _replacements;
        private readonly Stack<string> _scope;

        public CompactExpressionVisitor(ProjectionExpression projection)
        {
            _replacements = projection
                .Bindings
                .OfType<IBindingSqlExpression>()
                .ToDictionary(
                    binding => binding.Name,
                    NamedBindingExpression.Unwrap,
                    StringComparer.OrdinalIgnoreCase);

            _scope = new Stack<string>();
        }

        protected override ISqlExpression VisitSimpleBinding(SimpleBindingExpression simpleBindingExpression)
        {
            using (Disposable.Create(_scope, Push, Pop))
            {
                return simpleBindingExpression.Source is ParameterExpression
                    && _replacements.TryGetValue(_scope.ToString("_"), out var replacement)
                    && replacement.Type == simpleBindingExpression.Type
                    ? replacement
                    : base.VisitSimpleBinding(simpleBindingExpression);
            }

            void Push(Stack<string> scope)
            {
                scope.Push(simpleBindingExpression.Member.Name);
            }

            void Pop(Stack<string> scope)
            {
                scope.Pop();
            }
        }

        protected override ISqlExpression VisitParameter(ParameterExpression parameterExpression)
        {
            return _replacements.Count == 1
                   && _replacements.Single().Value is { } binding
                ? binding
                : parameterExpression;
        }
    }
}