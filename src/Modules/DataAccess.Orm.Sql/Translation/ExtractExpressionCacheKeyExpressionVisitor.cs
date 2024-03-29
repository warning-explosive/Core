namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using Basics;
    using Linq;

    internal class ExtractExpressionCacheKeyExpressionVisitor : ExpressionVisitor
    {
        private string? _cacheKey;

        private ExtractExpressionCacheKeyExpressionVisitor()
        {
        }

        public static bool TryGetCacheKey(Expression expression, [NotNullWhen(true)] out string? cacheKey)
        {
            var visitor = new ExtractExpressionCacheKeyExpressionVisitor();

            _ = visitor.Visit(expression);

            cacheKey = visitor._cacheKey;

            return cacheKey != null;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var method = node.Method.GenericMethodDefinitionOrSelf();

            var isCachedExpressionMethod = method == LinqMethods.CachedExpression()
                                           || method == LinqMethods.CachedInsertExpression()
                                           || method == LinqMethods.CachedUpdateExpression()
                                           || method == LinqMethods.CachedDeleteExpression();

            if (isCachedExpressionMethod
                && node.Arguments[1] is ConstantExpression constantExpression
                && constantExpression.Value is string cacheKey)
            {
                _cacheKey = cacheKey;
            }

            return base.VisitMethodCall(node);
        }
    }
}