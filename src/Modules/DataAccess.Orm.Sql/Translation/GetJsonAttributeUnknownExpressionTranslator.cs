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
                context.WithinScope(
                    new ParenthesesExpression(),
                    () => context.WithinScope(
                        new JsonAttributeExpression(expression.Type),
                        () => visitor.Visit(methodCallExpression.Arguments)));

                return true;
            }

            return false;
        }
    }
}