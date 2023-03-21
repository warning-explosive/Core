namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Expressions;
    using Linq;
    using BinaryExpression = Expressions.BinaryExpression;
    using MethodCallExpression = System.Linq.Expressions.MethodCallExpression;

    [Component(EnLifestyle.Singleton)]
    internal class ContainsUnknownExpressionTranslator : IUnknownExpressionTranslator,
                                                         ICollectionResolvable<IUnknownExpressionTranslator>
    {
        public bool TryTranslate(
            TranslationContext context,
            Expression expression,
            ExpressionVisitor visitor)
        {
            if (expression is MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.Method.GenericMethodDefinitionOrSelf() == LinqMethods.EnumerableContains())
                {
                    context.WithinScope(
                        new BinaryExpression(typeof(bool), BinaryOperator.Contains),
                        () => visitor.Visit(methodCallExpression.Arguments));

                    return true;
                }

                if (IsCollectionContains(methodCallExpression.Method))
                {
                    context.WithinScope(
                        new BinaryExpression(typeof(bool), BinaryOperator.Contains),
                        () =>
                        {
                            visitor.Visit(methodCallExpression.Object);
                            visitor.Visit(methodCallExpression.Arguments);
                        });

                    return true;
                }
            }

            return false;

            static bool IsCollectionContains(MethodInfo methodInfo)
            {
                return typeof(ICollection).IsAssignableFrom(methodInfo.DeclaringType)
                    && methodInfo.Name.Equals(nameof(ICollection<object>.Contains), StringComparison.OrdinalIgnoreCase)
                    && methodInfo.GetParameters().Length == 1;
            }
        }
    }
}