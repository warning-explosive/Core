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
                context.WithinScope(
                    new Expressions.BinaryExpression(typeof(bool), BinaryOperator.ArrayIntersection),
                    () =>
                    {
                        visitor.Visit(methodCallExpression.Object);
                        visitor.Visit(methodCallExpression.Arguments);
                    });

                return true;
            }

            return false;
        }
    }
}