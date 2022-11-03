namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using Basics;
    using Linq;

    internal class UnwrapScalarQueryableMethodsWithPredicateExpressionVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo Where = LinqMethods.QueryableWhere();

        private static readonly MethodInfo Single2 = LinqMethods.QueryableSingle2();
        private static readonly MethodInfo SingleOrDefault2 = LinqMethods.QueryableSingleOrDefault2();
        private static readonly MethodInfo First2 = LinqMethods.QueryableFirst2();
        private static readonly MethodInfo FirstOrDefault2 = LinqMethods.QueryableFirstOrDefault2();
        private static readonly MethodInfo Any2 = LinqMethods.QueryableAny2();
        private static readonly MethodInfo Count2 = LinqMethods.QueryableCount2();

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

            var isScalarWithPredicate = method == Single2
                                        || method == SingleOrDefault2
                                        || method == First2
                                        || method == FirstOrDefault2
                                        || method == Any2
                                        || method == Count2;

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
                Where.MakeGenericMethod(sourceType),
                node.Arguments[0],
                node.Arguments[1]);

            return Expression.Call(
                null,
                Map[method].MakeGenericMethod(sourceType),
                where);
        }
    }
}