namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Basics.Enumerations;
    using Expressions;
    using Linq;

    [Component(EnLifestyle.Singleton)]
    internal class OrderByLinqExpressionVisitor : ILinqExpressionVisitor,
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

            if (method == LinqMethods.QueryableOrderBy()
                || method == LinqMethods.QueryableOrderByDescending()
                || method == LinqMethods.QueryableThenBy()
                || method == LinqMethods.QueryableThenByDescending())
            {
                visitor.Visit(methodCallExpression.Arguments[0]);
                var source = context.SqlExpression;

                if (source is ProjectionExpression { Source: NamedSourceExpression namedSourceExpression } projectionExpression)
                {
                    var parameterExpression = namedSourceExpression.Parameter;
                    ISqlExpression orderByExpressionAccessor;

                    using (context.OpenParameterScope(parameterExpression))
                    {
                        visitor.Visit(methodCallExpression.Arguments[1]);
                        orderByExpressionAccessor = context.SqlExpression;
                    }

                    var direction = method == LinqMethods.QueryableOrderBy() || method == LinqMethods.QueryableThenBy()
                        ? EnOrderingDirection.Asc
                        : EnOrderingDirection.Desc;

                    var orderByExpressionExpression = new OrderByExpressionExpression(orderByExpressionAccessor, direction);
                    projectionExpression.OrderByExpression ??= new OrderByExpression(new List<OrderByExpressionExpression>());
                    ((ICollection<OrderByExpressionExpression>)projectionExpression.OrderByExpression.Expressions).Add(orderByExpressionExpression);
                    context.Remember(projectionExpression);

                    return true;
                }
            }

            return false;
        }
    }
}