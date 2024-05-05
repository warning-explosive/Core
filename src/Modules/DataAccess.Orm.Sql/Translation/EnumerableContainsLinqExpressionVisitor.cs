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

    [Component(EnLifestyle.Singleton)]
    internal class EnumerableContainsLinqExpressionVisitor : ILinqExpressionVisitor,
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

            if (method == LinqMethods.EnumerableContains())
            {
                visitor.Visit(methodCallExpression.Arguments[1]);
                var left = context.SqlExpression;
                visitor.Visit(methodCallExpression.Arguments[0]);
                var right = new ParenthesesExpression(context.SqlExpression);

                var binaryExpression = new Expressions.BinaryExpression(typeof(bool), BinaryOperator.Contains, left, right);
                context.Remember(binaryExpression);

                return true;
            }

            if (IsCollectionContains(method))
            {
                visitor.Visit(methodCallExpression.Arguments[0]);
                var left = context.SqlExpression;
                visitor.Visit(methodCallExpression.Object);
                var right = new ParenthesesExpression(context.SqlExpression);

                var binaryExpression = new Expressions.BinaryExpression(typeof(bool), BinaryOperator.Contains, left, right);
                context.Remember(binaryExpression);

                return true;
            }

            return false;
        }

        private static bool IsCollectionContains(MethodInfo methodInfo)
        {
            return typeof(ICollection).IsAssignableFrom(methodInfo.DeclaringType)
                   && methodInfo.Name.Equals(nameof(ICollection<object>.Contains), StringComparison.OrdinalIgnoreCase)
                   && methodInfo.GetParameters().Length == 1;
        }
    }
}