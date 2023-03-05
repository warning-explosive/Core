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
        private readonly IReadOnlyDictionary<string, ITypedSqlExpression> _replacements;
        private readonly Stack<string> _scope;

        public CompactExpressionVisitor(ProjectionExpression projection)
        {
            _replacements = projection
                .Expressions
                .ToDictionary(
                    expression => (expression as ColumnExpression)?.Name ?? (expression as RenameExpression).Name,
                    expression => (ITypedSqlExpression)RenameExpression.UnwrapRenames(expression),
                    StringComparer.OrdinalIgnoreCase);

            _scope = new Stack<string>();
        }

        protected override ISqlExpression VisitColumnExpression(ColumnExpression columnExpression)
        {
            using (Disposable.Create(_scope, Push, Pop))
            {
                return columnExpression.Source is ParameterExpression
                    && _replacements.TryGetValue(_scope.ToString("_"), out var replacement)
                    && replacement.Type == columnExpression.Type
                    ? replacement
                    : base.VisitColumnExpression(columnExpression);
            }

            void Push(Stack<string> scope)
            {
                scope.Push(columnExpression.Member.Name);
            }

            void Pop(Stack<string> scope)
            {
                scope.Pop();
            }
        }

        protected override ISqlExpression VisitParameter(ParameterExpression parameterExpression)
        {
            return _replacements.Count == 1
                   && _replacements.Single().Value is { } expression
                ? expression
                : parameterExpression;
        }
    }
}