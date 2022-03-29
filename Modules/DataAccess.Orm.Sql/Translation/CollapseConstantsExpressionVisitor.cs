namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Linq;
    using System.Linq.Expressions;
    using Api.Model;
    using Basics;
    using Orm.Linq;

    internal class CollapseConstantsExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var visitedNode = base.VisitMethodCall(node);

            if (visitedNode is ConstantExpression)
            {
                return visitedNode;
            }

            if (visitedNode is MethodCallExpression { Object: ConstantExpression or null } methodCallExpression
                && methodCallExpression.Arguments.All(argument => argument is ConstantExpression))
            {
                var method = node.Method.IsGenericMethod
                    ? node.Method.GetGenericMethodDefinition()
                    : node.Method;

                var itemType = node.Type.UnwrapTypeParameter(typeof(IQueryable<>));

                var isQueryRoot = itemType.IsClass
                    && itemType.IsSubclassOfOpenGeneric(typeof(IUniqueIdentified<>))
                    && method == LinqMethods.All(itemType, itemType.UnwrapTypeParameter(typeof(IUniqueIdentified<>)));

                if (!isQueryRoot)
                {
                    return visitedNode.CollapseConstantExpression();
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

            if (visitedNode is MemberExpression { Expression: ConstantExpression })
            {
                return node.CollapseConstantExpression();
            }

            if (visitedNode is MemberExpression { Expression: null })
            {
                return node.CollapseConstantExpression();
            }

            if (node.Expression is ConstantExpression)
            {
                return node.CollapseConstantExpression();
            }

            return visitedNode;
        }
    }
}