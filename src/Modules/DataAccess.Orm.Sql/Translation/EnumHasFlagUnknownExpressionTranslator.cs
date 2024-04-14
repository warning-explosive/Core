namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Linq.Expressions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Expressions;
    using Linq;
    using MethodCallExpression = System.Linq.Expressions.MethodCallExpression;

    [Component(EnLifestyle.Singleton)]
    internal class EnumHasFlagUnknownExpressionTranslator : IUnknownExpressionTranslator,
                                                            ICollectionResolvable<IUnknownExpressionTranslator>
    {
        public bool TryTranslate(
            TranslationContext context,
            Expression expression,
            ExpressionVisitor visitor)
        {
            if (expression is MethodCallExpression methodCallExpression
                && methodCallExpression.Method == LinqMethods.EnumHasFlag())
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