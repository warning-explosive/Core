namespace SpaceEngineers.Core.Basics;

using System.Linq.Expressions;

public static partial class PredicateExtensions
{
    public static Expression<Func<T, bool>> And<T>(
        this Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        var parameter = left.Parameters.Single();

        return Expression.Lambda<Func<T, bool>>(
            Expression.AndAlso(
                left.Body.ReplaceParameter(parameter),
                right.Body.ReplaceParameter(parameter)),
            parameter);
    }

    public static Expression<Func<T, bool>> Or<T>(
        this Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        var parameter = left.Parameters.Single();

        return Expression.Lambda<Func<T, bool>>(
            Expression.OrElse(
                left.Body.ReplaceParameter(parameter),
                right.Body.ReplaceParameter(parameter)),
            parameter);
    }
}