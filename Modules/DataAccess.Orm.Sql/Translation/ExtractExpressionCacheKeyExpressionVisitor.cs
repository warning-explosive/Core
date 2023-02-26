namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using Basics;
    using Orm.Linq;

    internal class ExtractExpressionCacheKeyExpressionVisitor : ExpressionVisitor
    {
        private string? _cacheKey;

        public bool TryGetCacheKey(Expression expression, [NotNullWhen(true)] out string? cacheKey)
        {
            _ = Visit(expression);

            cacheKey = _cacheKey;

            return cacheKey != null;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var method = node.Method.GenericMethodDefinitionOrSelf();

            if (method == LinqMethods.CachedExpression()
                && node.Arguments[1] is ConstantExpression constantExpression
                && constantExpression.Value is string cacheKey)
            {
                _cacheKey = cacheKey;
            }

            return base.VisitMethodCall(node);
        }
    }
}