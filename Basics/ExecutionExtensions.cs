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
            typeof(AccessViolationException)
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
        /// Can be exception caught or not
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <returns>Can be exception caught or not sign</returns>
        internal static bool CanBeCaught(Exception exception)
        {
            return !ExceptionTypesForSkip.Contains(exception.GetType());
        }
    }
}