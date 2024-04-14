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
    internal class IsNotNullUnknownExpressionTranslator : IUnknownExpressionTranslator,
                                                          ICollectionResolvable<IUnknownExpressionTranslator>
    {
        public bool TryTranslate(
            TranslationContext context,
            Expression expression,
            ExpressionVisitor visitor)
        {
            if (expression is MethodCallExpression methodCallExpression
                && methodCallExpression.Method == LinqMethods.IsNotNull())
            {
                visitor.Visit(methodCallExpression.Arguments[0]);
                var left = context.SqlExpression;
                var binaryExpression = new BinaryExpression(typeof(bool), BinaryOperator.IsNot, left, new NullExpression());
                context.Remember(binaryExpression);

                return true;
            }

            return false;
        }
    }
}