namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Linq.Expressions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Linq;

    [Component(EnLifestyle.Singleton)]
    internal class CachedExpressionLinqExpressionVisitor : ILinqExpressionVisitor,
                                                           ICollectionResolvable<ILinqExpressionVisitor>
    {
        public bool TryVisit(
            ExpressionVisitor visitor,
            TranslationContext context,
            Expression expression)
        {
            if (expression is not MethodCallExpression methodCallExpression)
            {
                return false;
            }

            var method = methodCallExpression.Method.GenericMethodDefinitionOrSelf();

            if (method == LinqMethods.CachedExpression()
                || method == LinqMethods.CachedInsertExpression()
                || method == LinqMethods.CachedUpdateExpression()
                || method == LinqMethods.CachedDeleteExpression()
                || method == LinqMethods.WithDependencyContainer())
            {
                visitor.Visit(methodCallExpression.Arguments[0]);

                return true;
            }

            return false;
        }
    }
}