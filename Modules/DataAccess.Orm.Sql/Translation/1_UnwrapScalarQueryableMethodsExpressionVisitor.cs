namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Linq;

    [Component(EnLifestyle.Singleton)]
    internal class UnwrapScalarQueryableMethodsExpressionVisitor : ExpressionVisitor,
                                                                   ILinqExpressionPreprocessor,
                                                                   ICollectionResolvable<ILinqExpressionPreprocessor>
    {
        private static readonly IReadOnlyDictionary<MethodInfo, MethodInfo> Map =
            new Dictionary<MethodInfo, MethodInfo>
            {
                [LinqMethods.QueryableSingle2()] = LinqMethods.QueryableSingle(),
                [LinqMethods.QueryableSingleOrDefault2()] = LinqMethods.QueryableSingleOrDefault(),
                [LinqMethods.QueryableFirst2()] = LinqMethods.QueryableFirst(),
                [LinqMethods.QueryableFirstOrDefault2()] = LinqMethods.QueryableFirstOrDefault(),
                [LinqMethods.QueryableAny2()] = LinqMethods.QueryableAny(),
                [LinqMethods.QueryableCount2()] = LinqMethods.QueryableCount()
            };

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var method = node.Method.GenericMethodDefinitionOrSelf();

            var isScalarWithPredicate = method == LinqMethods.QueryableSingle2()
                                        || method == LinqMethods.QueryableSingleOrDefault2()
                                        || method == LinqMethods.QueryableFirst2()
                                        || method == LinqMethods.QueryableFirstOrDefault2()
                                        || method == LinqMethods.QueryableAny2()
                                        || method == LinqMethods.QueryableCount2();

            if (!isScalarWithPredicate)
            {
                return node;
            }

            return ReplaceWithPredicate(node, method);
        }

        private static Expression ReplaceWithPredicate(MethodCallExpression node, MethodInfo method)
        {
            var sourceType = node.Method.GetGenericArguments()[0];

            var where = Expression.Call(
                null,
                LinqMethods.QueryableWhere().MakeGenericMethod(sourceType),
                node.Arguments[0],
                node.Arguments[1]);

            return Expression.Call(
                null,
                Map[method].MakeGenericMethod(sourceType),
                where);
        }
    }
}