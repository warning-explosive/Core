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
    internal class HasJsonAttributeLinqExpressionVisitor : ILinqExpressionVisitor,
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

            if (method == LinqMethods.HasJsonAttribute())
            {
                visitor.Visit(methodCallExpression.Arguments[0]);
                var left = context.SqlExpression;
                visitor.Visit(methodCallExpression.Arguments[1]);
                var right = context.SqlExpression;
                var binaryExpression = new Expressions.BinaryExpression(typeof(void), BinaryOperator.HasJsonAttribute, left, right);
                context.Remember(binaryExpression);

                return true;
            }

            return false;
        }
    }
}