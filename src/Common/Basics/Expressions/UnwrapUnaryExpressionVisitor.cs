namespace SpaceEngineers.Core.Basics.Expressions;

using System.Linq;
using System.Linq.Expressions;

public class UnwrapUnaryExpressionVisitor : ExpressionVisitor
{
    private readonly ExpressionType[] _expressionTypes;

    private UnwrapUnaryExpressionVisitor(ExpressionType[] expressionTypes)
    {
        _expressionTypes = expressionTypes;
    }

    public static Expression Unwrap(Expression expression, ExpressionType[] expressionTypes)
    {
        return new UnwrapUnaryExpressionVisitor(expressionTypes).Visit(expression);
    }

    protected override Expression VisitUnary(UnaryExpression node)
    {
        return _expressionTypes.Contains(node.NodeType)
            ? node.Operand
            : base.VisitUnary(node);
    }
}