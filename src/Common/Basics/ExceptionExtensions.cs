namespace SpaceEngineers.Core.Basics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;

public static class ExceptionExtensions
{
    public static Exception Rethrow(this Exception exception)
    {
        ExceptionDispatchInfo.Capture(exception).Throw();
        return exception;
    }

    public static Exception RealException(this Exception exception)
    {
        return exception
            .Flatten()
            .First(ex => ex is not TargetInvocationException
                         && ex is not AggregateException);
    }

    public static IEnumerable<Exception> Flatten(this Exception exception)
    {
        switch (exception)
        {
            case AggregateException a: return new[] { a }.Concat(a.Flatten().InnerExceptions.SelectMany(Flatten));
            default: return exception.InnerException != null
                ? new[] { exception }.Concat(Flatten(exception.InnerException))
                : [exception];
        }
    }
}