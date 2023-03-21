namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Basics.Attributes;
    using Linq;

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
                var method = methodCallExpression.Method.GenericMethodDefinitionOrSelf();

                if (method == LinqMethods.RepositoryAll()
                    || method == LinqMethods.RepositoryInsert()
                    || method == LinqMethods.RepositoryUpdate()
                    || method == LinqMethods.RepositoryDelete())
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

        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            var array = Array.CreateInstance(node.Type.GetElementType(), node.Expressions.Count);

            var i = 0;

            foreach (var expression in node.Expressions)
            {
                var value = ((ConstantExpression)Visit(expression)).Value;
                array.SetValue(value, i);
                i++;
            }

            return Expression.Constant(array);
        }

        protected override Expression VisitListInit(ListInitExpression node)
        {
            var visitedNode = base.VisitListInit(node);

            if (visitedNode is ListInitExpression listInitExpression)
            {
                var arguments = listInitExpression
                    .NewExpression
                    .Arguments
                    .Select(argument => ((ConstantExpression)argument).Value)
                    .ToArray();

                var collection = listInitExpression.NewExpression.Constructor.Invoke(arguments);

                foreach (var initializer in listInitExpression.Initializers)
                {
                    var addArguments = initializer
                        .Arguments
                        .Select(argument => ((ConstantExpression)argument).Value)
                        .ToArray();

                    initializer.AddMethod.Invoke(collection, addArguments);
                }

                return Expression.Constant(collection);
            }

            return visitedNode;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var visitedNode = base.VisitBinary(node);

            if (visitedNode is BinaryExpression binaryExpression
                && (binaryExpression.NodeType == ExpressionType.LessThan
                    || binaryExpression.NodeType == ExpressionType.LessThanOrEqual
                    || binaryExpression.NodeType == ExpressionType.GreaterThan
                    || binaryExpression.NodeType == ExpressionType.GreaterThanOrEqual
                    || binaryExpression.NodeType == ExpressionType.Equal
                    || binaryExpression.NodeType == ExpressionType.NotEqual
                    || binaryExpression.NodeType == ExpressionType.ExclusiveOr))
            {
                if (binaryExpression.Left is UnaryExpression leftUnaryExpression
                    && leftUnaryExpression.NodeType == ExpressionType.Convert
                    && leftUnaryExpression.Operand.Type.IsEnum
                    && binaryExpression.Right is ConstantExpression rightConstantExpression)
                {
                    var enumType = Enum.GetUnderlyingType(leftUnaryExpression.Operand.Type);

                    return Expression.MakeBinary(
                        binaryExpression.NodeType,
                        Expression.Convert(leftUnaryExpression.Operand, enumType),
                        Expression.Convert(Expression.Constant(Enum.ToObject(leftUnaryExpression.Operand.Type, rightConstantExpression.Value)), enumType));
                }

                if (binaryExpression.Right is UnaryExpression rightUnaryExpression
                    && rightUnaryExpression.NodeType == ExpressionType.Convert
                    && rightUnaryExpression.Operand.Type.IsEnum
                    && binaryExpression.Left is ConstantExpression leftConstantExpression)
                {
                    var enumType = Enum.GetUnderlyingType(rightUnaryExpression.Operand.Type);

                    return Expression.MakeBinary(
                        binaryExpression.NodeType,
                        Expression.Convert(Expression.Constant(Enum.ToObject(rightUnaryExpression.Operand.Type, leftConstantExpression.Value)), enumType),
                        Expression.Convert(rightUnaryExpression.Operand, enumType));
                }
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