namespace SpaceEngineers.Core.Basics;

using System.Linq.Expressions;
using Expressions;

public static partial class ExpressionExtensions
{
    public static Expression ReplaceParameter(
        this Expression expression,
        ParameterExpression parameterExpression)
    {
        return ReplaceParameterVisitor.Replace(expression, parameterExpression);
    }

    public static Expression UnwrapUnaryExpression(
        this Expression expression)
    {
        var expressionTypes = new[]
        {
            ExpressionType.Quote,
            ExpressionType.Convert,
            ExpressionType.ConvertChecked
        };

        return UnwrapUnaryExpressionVisitor.Unwrap(expression, expressionTypes);
    }
}