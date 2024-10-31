namespace SpaceEngineers.Core.Basics;

using System;
using System.Linq;
using System.Linq.Expressions;

public static class PredicateExtensions
{
    public static Func<T, bool> Not<T>(this Func<T, bool> function)
    {
        return input => !function.Invoke(input);
    }

    public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> expression)
    {
        var parameter = expression.Parameters.Single();
        return Expression.Lambda<Func<T, bool>>(Expression.Not(expression.Body), parameter);
    }

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