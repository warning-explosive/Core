namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Attributes;
    using Orm.Linq;

    [Component(EnLifestyle.Singleton)]
    [After(typeof(UnwrapScalarQueryableMethodsExpressionVisitor))]
    internal class CollapseConstantsExpressionVisitor : ExpressionVisitor,
                                                        ILinqExpressionPreprocessor,
                                                        ICollectionResolvable<ILinqExpressionPreprocessor>
    {
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var visitedNode = base.VisitMethodCall(node);

            if (visitedNode is ConstantExpression)
            {
                return visitedNode;
            }

            if (visitedNode is MethodCallExpression { Object: ConstantExpression or null } methodCallExpression)
            {
                if (methodCallExpression.Method.IsQueryRoot())
                {
                    return visitedNode;
                }

                var parameters = methodCallExpression
                    .Arguments
                    .OfType<ConstantExpression>()
                    .Select(argument => argument.Value)
                    .ToArray();

                if (parameters.Length != methodCallExpression.Arguments.Count)
                {
                    return visitedNode;
                }

                var target = (methodCallExpression.Object as ConstantExpression)?.Value;

                return Expression.Constant(methodCallExpression.Method.Invoke(target, parameters));
            }

            return visitedNode;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var visitedNode = base.VisitMember(node);

            if (visitedNode is ConstantExpression)
            {
                return visitedNode;
            }

            if (visitedNode is MemberExpression { Expression: ConstantExpression or null } memberExpression)
            {
                var target = (memberExpression.Expression as ConstantExpression)?.Value;

                return CollapseMemberInfo(memberExpression.Member, target);
            }

            return visitedNode;
        }

        private static ConstantExpression CollapseMemberInfo(MemberInfo member, object? target)
        {
            return Expression.Constant(member switch
            {
                PropertyInfo propertyInfo => propertyInfo.GetValue(target),
                FieldInfo fieldInfo => fieldInfo.GetValue(target),
                _ => throw new NotSupportedException(member.GetType().FullName)
            });
        }
    }
}