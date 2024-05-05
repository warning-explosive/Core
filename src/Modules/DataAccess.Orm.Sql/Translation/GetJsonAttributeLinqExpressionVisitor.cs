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
    internal class GetJsonAttributeLinqExpressionVisitor : ILinqExpressionVisitor,
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

            if (method == LinqMethods.GetJsonAttribute())
            {
                visitor.Visit(methodCallExpression.Arguments[0]);
                var source = context.SqlExpression;
                visitor.Visit(methodCallExpression.Arguments[1]);
                var accessor = context.SqlExpression;
                var jsonAttributeExpression = new JsonAttributeExpression(methodCallExpression.Type, source, accessor);
                var parenthesesExpression = new ParenthesesExpression(jsonAttributeExpression);
                context.Remember(parenthesesExpression);

                return true;
            }

            return false;
        }
    }
}