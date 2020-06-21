namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Linq;

    /// <summary>
    /// Execution extension methods
    /// </summary>
    public static class ExecutionExtensions
    {
        private static readonly Type[] ExceptionTypesForSkip =
        {
            typeof(StackOverflowException),
            typeof(OutOfMemoryException),
            typeof(OperationCanceledException),
        };

        /// <summary>
        /// Try execute client action
        /// </summary>
        /// <param name="clientAction">Client action</param>
        /// <returns>ActionExecutionInfo</returns>
        public static ActionExecutionInfo Try(this Action clientAction)
        {
            return new ActionExecutionInfo(clientAction);
        }

        /// <summary>
        /// Try execute client function
        /// </summary>
        /// <param name="clientFunction">Client function</param>
        /// <typeparam name="TResult">Function TResult type-argument</typeparam>
        /// <returns>ActionExecutionInfo</returns>
        public static FunctionExecutionInfo<TResult> Try<TResult>(this Func<TResult> clientFunction)
        {
            return new FunctionExecutionInfo<TResult>(clientFunction);
        }

        /// <summary>
        /// Invoke client function
        /// </summary>
        /// <param name="info">FunctionExecutionInfo</param>
        /// <param name="exceptionHandler">Exception handler</param>
        /// <typeparam name="TResult">Function TResult type-argument</typeparam>
        /// <returns>TResult</returns>
        public static TResult Invoke<TResult>(this FunctionExecutionInfo<TResult> info,
                                              Func<Exception, TResult>? exceptionHandler = null)
        {
            return info.InvokeInternal(exceptionHandler);
        }

        /// <summary>
        /// Can be exception catched or not
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <returns>Can be exception catched or not sign</returns>
        internal static bool CanBeCatched(Exception exception)
        {
            return !ExceptionTypesForSkip.Contains(exception.GetType());
        }
    }
}