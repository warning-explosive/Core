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
    internal class DistinctLinqExpressionVisitor : ILinqExpressionVisitor,
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

            if (method == LinqMethods.QueryableDistinct())
            {
                visitor.Visit(methodCallExpression.Arguments[0]);
                var projection = (ProjectionExpression)context.SqlExpression;
                projection.IsDistinct = true;
                context.Remember(projection);

                return true;
            }

            return false;
        }
    }
}