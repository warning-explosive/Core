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
    internal class ExcludeJsonAttributeLinqExpressionVisitor : ILinqExpressionVisitor,
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

            if (method == LinqMethods.ExcludeJsonAttribute())
            {
                visitor.Visit(methodCallExpression.Arguments[0]);
                var left = context.SqlExpression;
                visitor.Visit(methodCallExpression.Arguments[1]);
                var right = context.SqlExpression;
                var binaryExpression = new Expressions.BinaryExpression(typeof(void), BinaryOperator.Subtract, left, right);
                var parenthesesExpression = new ParenthesesExpression(binaryExpression);
                context.Remember(parenthesesExpression);

                return true;
            }

            return false;
        }
    }
}