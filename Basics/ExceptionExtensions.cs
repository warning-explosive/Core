namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Exception extension methods
    /// </summary>
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Unwrap TargetInvocationException
        /// </summary>
        /// <param name="exception">exception</param>
        /// <returns>Real exception hidden beside TargetInvocationException</returns>
        public static Exception RealException(this Exception exception)
        {
            while (exception is TargetInvocationException tex)
            {
                exception = tex.InnerException;
            }

            return exception;
        }
    }
}