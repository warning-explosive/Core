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
        private readonly ProjectionExpression _projection;
        private readonly Stack<string> _scope;

        private IReadOnlyDictionary<string, ITypedSqlExpression>? _replacements;

        private CompactExpressionVisitor(ProjectionExpression projection)
        {
            _projection = projection;
            _scope = new Stack<string>();
        }

        private IReadOnlyDictionary<string, ITypedSqlExpression> Replacements
        {
            get
            {
                _replacements ??= _projection
                    .Expressions
                    .ToDictionary(
                        expression => (expression as ColumnExpression)?.Name ?? (expression as RenameExpression).Name,
                        expression => (ITypedSqlExpression)RenameExpression.UnwrapRenames(expression),
                        StringComparer.OrdinalIgnoreCase);

                return _replacements;
            }
        }

        public static ISqlExpression Compact(ISqlExpression expression, ProjectionExpression projection)
        {
            return new CompactExpressionVisitor(projection).Visit(expression);
        }

        protected override ISqlExpression VisitColumnExpression(ColumnExpression columnExpression)
        {
            using (Disposable.Create(_scope, Push, Pop))
            {
                return columnExpression.Source is ParameterExpression
                    && Replacements.TryGetValue(_scope.ToString("_"), out var replacement)
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
            return Replacements.Count == 1
                   && Replacements.Single().Value is { } expression
                ? expression
                : parameterExpression;
        }
    }
}