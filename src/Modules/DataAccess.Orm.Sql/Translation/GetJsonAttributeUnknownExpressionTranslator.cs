namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Linq.Expressions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Expressions;
    using Linq;
    using MethodCallExpression = System.Linq.Expressions.MethodCallExpression;

    [Component(EnLifestyle.Singleton)]
    internal class GetJsonAttributeUnknownExpressionTranslator : IUnknownExpressionTranslator,
                                                                 ICollectionResolvable<IUnknownExpressionTranslator>
    {
        public bool TryTranslate(
            TranslationContext context,
            Expression expression,
            ExpressionVisitor visitor)
        {
            if (expression is MethodCallExpression methodCallExpression
                && methodCallExpression.Method.GenericMethodDefinitionOrSelf() == LinqMethods.GetJsonAttribute())
            {
                visitor.Visit(methodCallExpression.Arguments[0]);
                var source = context.SqlExpression;
                visitor.Visit(methodCallExpression.Arguments[1]);
                var accessor = context.SqlExpression;
                var jsonAttributeExpression = new JsonAttributeExpression(expression.Type, source, accessor);
                var parenthesesExpression = new ParenthesesExpression(jsonAttributeExpression);
                context.Remember(parenthesesExpression);

                return true;
            }

            return false;
        }
    }
}