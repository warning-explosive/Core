namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Linq.Expressions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Expressions;
    using Linq;
    using BinaryExpression = Expressions.BinaryExpression;
    using MethodCallExpression = System.Linq.Expressions.MethodCallExpression;

    [Component(EnLifestyle.Singleton)]
    internal class LikeUnknownExpressionTranslator : IUnknownExpressionTranslator,
                                                     ICollectionResolvable<IUnknownExpressionTranslator>
    {
        public bool TryTranslate(
            TranslationContext context,
            Expression expression,
            ExpressionVisitor visitor)
        {
            if (expression is MethodCallExpression methodCallExpression
                && methodCallExpression.Method == LinqMethods.Like())
            {
                visitor.Visit(methodCallExpression.Arguments[0]);
                var left = context.SqlExpression;
                visitor.Visit(methodCallExpression.Arguments[1]);
                var right = context.SqlExpression;
                var binaryExpression = new BinaryExpression(typeof(bool), BinaryOperator.Like, left, right);
                context.Remember(binaryExpression);

                return true;
            }

            return false;
        }
    }
}