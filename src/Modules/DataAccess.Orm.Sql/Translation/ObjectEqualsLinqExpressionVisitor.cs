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

    [Component(EnLifestyle.Singleton)]
    internal class ObjectEqualsLinqExpressionVisitor : ILinqExpressionVisitor,
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

            if (methodCallExpression.Method == LinqMethods.ObjectEquals())
            {
                visitor.Visit(methodCallExpression.Arguments[0]);
                var left = context.SqlExpression;
                visitor.Visit(methodCallExpression.Arguments[1]);
                var right = context.SqlExpression;
                var binaryExpression = new Expressions.BinaryExpression(typeof(bool), BinaryOperator.Equal, left, right);
                context.Remember(binaryExpression);

                return true;
            }

            if (IsInstanceEquals(methodCallExpression.Method))
            {
                visitor.Visit(methodCallExpression.Object);
                var left = context.SqlExpression;
                visitor.Visit(methodCallExpression.Arguments[0]);
                var right = context.SqlExpression;
                var binaryExpression = new Expressions.BinaryExpression(typeof(bool), BinaryOperator.Equal, left, right);
                context.Remember(binaryExpression);

                return true;
            }

            return false;
        }

        private static bool IsInstanceEquals(MethodInfo method)
        {
            return !method.IsStatic
                   && method.DeclaringType == typeof(object)
                   && method.Name.Equals(nameof(object.Equals), StringComparison.OrdinalIgnoreCase);
        }
    }
}