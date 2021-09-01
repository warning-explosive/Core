namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.ExceptionServices;

    /// <summary>
    /// Exception extension methods
    /// </summary>
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Rethrows exception and keeps original stack trace
        /// </summary>
        /// <param name="exception">Original exception</param>
        /// <returns>Exception to throw</returns>
        public static Exception Rethrow(this Exception exception)
        {
            ExceptionDispatchInfo.Capture(exception).Throw();
            return exception;
        }

        /// <summary>
        /// Unwrap TargetInvocationException
        /// </summary>
        /// <param name="exception">exception</param>
        /// <returns>Real exception hidden beside TargetInvocationException</returns>
        public static Exception RealException(this Exception exception)
        {
            return exception
                .Flatten()
                .First(ex => ex is not TargetInvocationException
                             && ex is not AggregateException);
        }

        /// <summary>
        /// Gets flatten exception object hierarchy
        /// </summary>
        /// <param name="exception">Source exception</param>
        /// <returns>Flatten exception object hierarchy</returns>
        public static IEnumerable<Exception> Flatten(this Exception exception)
        {
            switch (exception)
            {
                case AggregateException a: return new[] { a }.Concat(a.Flatten().InnerExceptions.SelectMany(Flatten));
                default: return exception.InnerException != null
                    ? new[] { exception }.Concat(Flatten(exception.InnerException))
                    : new[] { exception };
            }
        }
    }
}