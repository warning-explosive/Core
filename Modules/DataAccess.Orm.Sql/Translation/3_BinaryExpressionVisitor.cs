namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Linq.Expressions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Basics.Attributes;
    using Linq;

    [Component(EnLifestyle.Singleton)]
    [After(typeof(CollapseConstantsExpressionVisitor))]
    internal class BinaryExpressionVisitor : ExpressionVisitor,
                                             ILinqExpressionPreprocessor,
                                             ICollectionResolvable<ILinqExpressionPreprocessor>
    {
        protected override Expression VisitBinary(BinaryExpression node)
        {
            node = node.Left is ConstantExpression && node.Right is not ConstantExpression
                ? Expression.MakeBinary(node.NodeType, node.Right, node.Left)
                : node;

            node = (BinaryExpression)base.VisitBinary(node);

            if ((node.NodeType == ExpressionType.Equal || node.NodeType == ExpressionType.NotEqual)
                && node.Right is ConstantExpression
                && (node.Right.Type.IsReference() || node.Right.Type.IsNullable())
                && node.Right.Type.IsPrimitive())
            {
                return Expression.Condition(
                    Expression.Call(null, LinqMethods.IsNull(), node.Right),
                    Expression.Call(null, node.NodeType == ExpressionType.Equal ? LinqMethods.IsNull() : LinqMethods.IsNotNull(), node.Left),
                    Expression.MakeBinary(node.NodeType, node.Left, node.Right));
            }

            return node;
        }
    }
}