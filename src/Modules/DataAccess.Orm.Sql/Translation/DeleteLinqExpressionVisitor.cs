namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Linq.Expressions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Expressions;
    using Linq;

    [Component(EnLifestyle.Singleton)]
    internal class DeleteLinqExpressionVisitor : ILinqExpressionVisitor,
                                                 ICollectionResolvable<ILinqExpressionVisitor>
    {
        public bool TryVisit(
            ExpressionVisitor visitor,
            TranslationContext context,
            Expression expression)
        {
            if (expression is not System.Linq.Expressions.MethodCallExpression methodCallExpression)
            {
                return false;
            }

            var method = methodCallExpression.Method.GenericMethodDefinitionOrSelf();

            if (method == LinqMethods.RepositoryDelete())
            {
                var itemType = methodCallExpression.Type.ExtractQueryableItemType();

                var deleteExpression = new DeleteExpression(itemType, null);
                context.Remember(deleteExpression);

                return true;
            }

            return false;
        }
    }
}