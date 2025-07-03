namespace SpaceEngineers.Core.Basics;

using System;
using System.Linq;
using System.Linq.Expressions;

public static partial class PredicateExtensions
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
}