namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

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
        /// Try execute client's action
        /// </summary>
        /// <param name="clientAction">Client action</param>
        /// <returns>ActionExecutionInfo</returns>
        public static ActionExecutionInfo Try(this Action clientAction)
        {
            return new ActionExecutionInfo(clientAction);
        }

        /// <summary>
        /// Try execute client's function
        /// </summary>
        /// <param name="clientFunction">Client function</param>
        /// <typeparam name="TResult">Function TResult type-argument</typeparam>
        /// <returns>FunctionExecutionInfo</returns>
        public static FunctionExecutionInfo<TResult> Try<TResult>(this Func<TResult> clientFunction)
        {
            return new FunctionExecutionInfo<TResult>(clientFunction);
        }

        /// <summary>
        /// Try execute client's asynchronous operation
        /// </summary>
        /// <param name="clientAsyncOperationFactory">Client async operation factory</param>
        /// <param name="configureAwait">Configure await option</param>
        /// <returns>AsyncOperationExecutionInfo</returns>
        public static AsyncOperationExecutionInfo TryAsync(Func<Task> clientAsyncOperationFactory, bool configureAwait)
        {
            return new AsyncOperationExecutionInfo(clientAsyncOperationFactory, configureAwait);
        }

        /// <summary>
        /// Try execute client's asynchronous operation
        /// </summary>
        /// <param name="clientAsyncOperationFactory">Client async operation factory</param>
        /// <param name="configureAwait">Configure await option</param>
        /// <typeparam name="TResult">Async operation result type-argument</typeparam>
        /// <returns>AsyncOperationExecutionInfo</returns>
        public static AsyncGenericOperationExecutionInfo<TResult> TryAsync<TResult>(Func<Task<TResult>> clientAsyncOperationFactory, bool configureAwait)
        {
            return new AsyncGenericOperationExecutionInfo<TResult>(clientAsyncOperationFactory, configureAwait);
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