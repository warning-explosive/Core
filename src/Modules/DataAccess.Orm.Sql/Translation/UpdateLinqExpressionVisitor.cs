namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Expressions;
    using Linq;

    [Component(EnLifestyle.Singleton)]
    internal class UpdateLinqExpressionVisitor : ILinqExpressionVisitor,
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

            if (method == LinqMethods.RepositoryUpdate())
            {
                var itemType = methodCallExpression.Type.ExtractQueryableItemType();

                var updateExpression = new UpdateExpression(itemType, new List<Expressions.BinaryExpression>(), null);
                context.Remember(updateExpression);

                return true;
            }

            return false;
        }
    }
}