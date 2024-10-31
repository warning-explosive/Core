namespace SpaceEngineers.Core.Basics.Expressions;

using System.Linq.Expressions;

public class ReplaceParameterVisitor : ExpressionVisitor
{
    private readonly ParameterExpression _replacement;

    private ReplaceParameterVisitor(ParameterExpression replacement)
    {
        _replacement = replacement;
    }

    public static Expression Replace(Expression expression, ParameterExpression replacement)
    {
        return new ReplaceParameterVisitor(replacement).Visit(expression);
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        return _replacement;
    }
}