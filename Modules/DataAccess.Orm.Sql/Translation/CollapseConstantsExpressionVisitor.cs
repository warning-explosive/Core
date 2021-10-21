namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Linq.Expressions;
    using Basics;

    internal class CollapseConstantsExpressionVisitor : ExpressionVisitor
    {
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

            if (node.Expression is ConstantExpression)
            {
                return node.CollapseConstantExpression();
            }

            return visitedNode;
        }
    }
}