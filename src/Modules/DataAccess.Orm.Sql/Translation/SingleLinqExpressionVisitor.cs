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
    internal class SingleLinqExpressionVisitor : ILinqExpressionVisitor,
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

            if (method == LinqMethods.QueryableSingle()
                || method == LinqMethods.QueryableSingleOrDefault())
            {
                visitor.Visit(methodCallExpression.Arguments[0]);
                var source = context.SqlExpression;
                var rowsFetchLimitExpression = new RowsFetchLimitExpression(source, 2);
                context.Remember(rowsFetchLimitExpression);

                return true;
            }

            return false;
        }
    }
}