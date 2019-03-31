namespace SpaceEngineers.Core.Utilities.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Exception extension methods
    /// </summary>
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Extract original exceptions from source
        /// </summary>
        /// <param name="exception">Source exception</param>
        /// <returns>Extracted collection</returns>
        public static IEnumerable<Exception> Extract(this Exception exception)
        {
            if (exception == null)
            {
                return Enumerable.Empty<Exception>();
            }
            
            var result = new List<Exception>();

            result.AddRange(exception is AggregateException aggregateException
                                ? aggregateException.InnerExceptions.SelectMany(innerEx => innerEx.Extract())
                                : exception is TargetInvocationException targetInvocationException
                                    ? targetInvocationException.InnerException.Extract()
                                    : new[] { exception });

            return result.Distinct();
        }
    }
}