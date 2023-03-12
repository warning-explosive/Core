namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Linq.Expressions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Expressions;
    using Linq;
    using BinaryExpression = Expressions.BinaryExpression;
    using MethodCallExpression = System.Linq.Expressions.MethodCallExpression;

    [Component(EnLifestyle.Singleton)]
    internal class ConcatJsonObjectsUnknownExpressionTranslator : IUnknownExpressionTranslator,
                                                                  ICollectionResolvable<IUnknownExpressionTranslator>
    {
        public bool TryTranslate(
            TranslationContext context,
            Expression expression,
            ExpressionVisitor visitor)
        {
            if (expression is MethodCallExpression methodCallExpression
                && methodCallExpression.Method.GenericMethodDefinitionOrSelf() == LinqMethods.ConcatJsonObjects())
            {
                context.WithinScope(
                    new BinaryExpression(typeof(void), BinaryOperator.ConcatJsonObjects),
                    () => visitor.Visit(methodCallExpression.Arguments));

                return true;
            }

            return false;
        }
    }
}