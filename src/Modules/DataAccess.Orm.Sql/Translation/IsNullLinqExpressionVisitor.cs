namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Linq.Expressions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Expressions;
    using Linq;

    [Component(EnLifestyle.Singleton)]
    internal class IsNullLinqExpressionVisitor : ILinqExpressionVisitor,
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

            if (methodCallExpression.Method == LinqMethods.IsNull())
            {
                visitor.Visit(methodCallExpression.Arguments);
                var left = context.SqlExpression;
                var binaryExpression = new Expressions.BinaryExpression(typeof(bool), BinaryOperator.Is, left, new NullExpression());
                context.Remember(binaryExpression);

                return true;
            }

            return false;
        }
    }
}