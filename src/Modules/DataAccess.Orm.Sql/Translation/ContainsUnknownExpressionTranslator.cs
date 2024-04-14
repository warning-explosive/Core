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
            if (expression is MethodCallExpression methodCallExpression
                && (methodCallExpression.Method.GenericMethodDefinitionOrSelf() == LinqMethods.EnumerableContains()
                    || IsCollectionContains(methodCallExpression.Method)))
            {
                visitor.Visit(methodCallExpression.Arguments[1]);
                var left = context.SqlExpression;
                visitor.Visit(methodCallExpression.Arguments[0]);
                var right = context.SqlExpression;
                var binaryExpression = new BinaryExpression(typeof(bool), BinaryOperator.Contains, left, right);
                context.Remember(binaryExpression);

                return true;
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