namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Linq.Expressions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Expressions;
    using Linq;

    [Component(EnLifestyle.Singleton)]
    internal class EnumHasFlagLinqExpressionVisitor : ILinqExpressionVisitor,
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

            if (methodCallExpression.Method == LinqMethods.EnumHasFlag())
            {
                visitor.Visit(methodCallExpression.Object);
                var left = context.SqlExpression;
                visitor.Visit(methodCallExpression.Arguments[0]);
                var right = context.SqlExpression;
                var binaryExpression = new Expressions.BinaryExpression(typeof(bool), BinaryOperator.ArrayIntersection, left, right);
                context.Remember(binaryExpression);

                return true;
            }

            return false;
        }
    }
}