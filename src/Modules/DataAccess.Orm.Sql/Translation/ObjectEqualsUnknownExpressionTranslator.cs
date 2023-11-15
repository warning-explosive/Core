namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Expressions;
    using Linq;
    using BinaryExpression = Expressions.BinaryExpression;
    using MethodCallExpression = System.Linq.Expressions.MethodCallExpression;

    [Component(EnLifestyle.Singleton)]
    internal class ObjectEqualsUnknownExpressionTranslator : IUnknownExpressionTranslator,
                                                             ICollectionResolvable<IUnknownExpressionTranslator>
    {
        public bool TryTranslate(
            TranslationContext context,
            Expression expression,
            ExpressionVisitor visitor)
        {
            if (expression is MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.Method == LinqMethods.ObjectEquals())
                {
                    context.WithinScope(
                        new BinaryExpression(typeof(bool), BinaryOperator.Equal),
                        () => visitor.Visit(methodCallExpression.Arguments));

                    return true;
                }

                if (IsInstanceEquals(methodCallExpression.Method))
                {
                    context.WithinScope(
                        new BinaryExpression(typeof(bool), BinaryOperator.Equal),
                        () =>
                        {
                            visitor.Visit(methodCallExpression.Object);
                            visitor.Visit(methodCallExpression.Arguments);
                        });

                    return true;
                }
            }

            return false;

            static bool IsInstanceEquals(MethodInfo method)
            {
                return !method.IsStatic
                       && method.DeclaringType == typeof(object)
                       && method.Name.Equals(nameof(object.Equals), StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}