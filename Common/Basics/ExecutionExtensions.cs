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
        private static readonly Type[] ExceptionTypesForSkip = new[]
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
        /// <returns>StatelessActionExecutionInfo</returns>
        public static StatelessActionExecutionInfo Try(
            this Action clientAction)
        {
            return new StatelessActionExecutionInfo(clientAction);
        }

        /// <summary>
        /// Try execute client's action
        /// </summary>
        /// <param name="clientAction">Client action</param>
        /// <param name="state">State</param>
        /// <typeparam name="TState">TState type-argument</typeparam>
        /// <returns>StatelessActionExecutionInfo</returns>
        public static ActionExecutionInfo<TState> Try<TState>(
            this Action<TState> clientAction,
            TState state)
        {
            return new ActionExecutionInfo<TState>(state, clientAction);
        }

        /// <summary>
        /// Try execute client's function
        /// </summary>
        /// <param name="clientFunction">Client function</param>
        /// <typeparam name="TResult">Function TResult type-argument</typeparam>
        /// <returns>StatelessFunctionExecutionInfo</returns>
        public static StatelessFunctionExecutionInfo<TResult> Try<TResult>(
            this Func<TResult> clientFunction)
        {
            return new StatelessFunctionExecutionInfo<TResult>(clientFunction);
        }

        /// <summary>
        /// Try execute client's function
        /// </summary>
        /// <param name="clientFunction">Client function</param>
        /// <param name="state">State</param>
        /// <typeparam name="TState">TState type-argument</typeparam>
        /// <typeparam name="TResult">TResult type-argument</typeparam>
        /// <returns>FunctionExecutionInfo</returns>
        public static FunctionExecutionInfo<TState, TResult> Try<TState, TResult>(
            this Func<TState, TResult> clientFunction,
            TState state)
        {
            return new FunctionExecutionInfo<TState, TResult>(state, clientFunction);
        }

        /// <summary>
        /// Try execute client's asynchronous operation
        /// </summary>
        /// <param name="task">Async operation</param>
        /// <param name="configureAwait">Configure await option</param>
        /// <returns>AsyncOperationExecutionInfo</returns>
        public static AsyncOperationExecutionInfo TryAsync(
            this Task task,
            bool configureAwait = false)
        {
            return new AsyncOperationExecutionInfo(task, configureAwait);
        }

        /// <summary>
        /// Try execute client's asynchronous operation
        /// </summary>
        /// <param name="task">Async operation</param>
        /// <param name="configureAwait">Configure await option</param>
        /// <typeparam name="TResult">TResult type-argument</typeparam>
        /// <returns>AsyncOperationExecutionInfo</returns>
        public static AsyncOperationExecutionInfo<TResult> TryAsync<TResult>(
            this Task<TResult> task,
            bool configureAwait = false)
        {
            return new AsyncOperationExecutionInfo<TResult>(task, configureAwait);
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