namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Linq;
    using System.Threading;
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
        /// <returns>StatelessActionExecutionInfo</returns>
        public static StatelessActionExecutionInfo Try(
            Action clientAction)
        {
            return new StatelessActionExecutionInfo(clientAction);
        }

        /// <summary>
        /// Try execute client's action
        /// </summary>
        /// <param name="state">State</param>
        /// <param name="clientAction">Client action</param>
        /// <typeparam name="TState">TState type-argument</typeparam>
        /// <returns>StatelessActionExecutionInfo</returns>
        public static ActionExecutionInfo<TState> Try<TState>(
            TState state,
            Action<TState> clientAction)
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
            Func<TResult> clientFunction)
        {
            return new StatelessFunctionExecutionInfo<TResult>(clientFunction);
        }

        /// <summary>
        /// Try execute client's function
        /// </summary>
        /// <param name="state">State</param>
        /// <param name="clientFunction">Client function</param>
        /// <typeparam name="TState">TState type-argument</typeparam>
        /// <typeparam name="TResult">TResult type-argument</typeparam>
        /// <returns>FunctionExecutionInfo</returns>
        public static FunctionExecutionInfo<TState, TResult> Try<TState, TResult>(
            TState state,
            Func<TState, TResult> clientFunction)
        {
            return new FunctionExecutionInfo<TState, TResult>(state, clientFunction);
        }

        /// <summary>
        /// Try execute client's asynchronous operation
        /// </summary>
        /// <param name="clientAsyncOperationFactory">Client async operation factory</param>
        /// <param name="configureAwait">Configure await option</param>
        /// <returns>StatelessAsyncOperationExecutionInfo</returns>
        public static StatelessAsyncOperationExecutionInfo TryAsync(
            Func<CancellationToken, Task> clientAsyncOperationFactory,
            bool configureAwait = false)
        {
            return new StatelessAsyncOperationExecutionInfo(clientAsyncOperationFactory, configureAwait);
        }

        /// <summary>
        /// Try execute client's asynchronous operation
        /// </summary>
        /// <param name="state">State</param>
        /// <param name="clientAsyncOperationFactory">Client async operation factory</param>
        /// <param name="configureAwait">Configure await option</param>
        /// <typeparam name="TState">TState type-argument</typeparam>
        /// <returns>AsyncOperationExecutionInfo</returns>
        public static AsyncOperationExecutionInfo<TState> TryAsync<TState>(
            TState state,
            Func<TState, CancellationToken, Task> clientAsyncOperationFactory,
            bool configureAwait = false)
        {
            return new AsyncOperationExecutionInfo<TState>(state, clientAsyncOperationFactory, configureAwait);
        }

        /// <summary>
        /// Try execute client's asynchronous operation
        /// </summary>
        /// <param name="clientAsyncOperationFactory">Client async operation factory</param>
        /// <param name="configureAwait">Configure await option</param>
        /// <typeparam name="TResult">TResult type-argument</typeparam>
        /// <returns>StatelessGenericAsyncOperationExecutionInfo</returns>
        public static StatelessGenericAsyncOperationExecutionInfo<TResult> TryAsync<TResult>(
            Func<CancellationToken, Task<TResult>> clientAsyncOperationFactory,
            bool configureAwait = false)
        {
            return new StatelessGenericAsyncOperationExecutionInfo<TResult>(clientAsyncOperationFactory, configureAwait);
        }

        /// <summary>
        /// Try execute client's asynchronous operation
        /// </summary>
        /// <param name="state">State</param>
        /// <param name="clientAsyncOperationFactory">Client async operation factory</param>
        /// <param name="configureAwait">Configure await option</param>
        /// <typeparam name="TState">TState type-argument</typeparam>
        /// <typeparam name="TResult">TResult type-argument</typeparam>
        /// <returns>GenericAsyncOperationExecutionInfo</returns>
        public static GenericAsyncOperationExecutionInfo<TState, TResult> TryAsync<TState, TResult>(
            TState state,
            Func<TState, CancellationToken, Task<TResult>> clientAsyncOperationFactory,
            bool configureAwait = false)
        {
            return new GenericAsyncOperationExecutionInfo<TState, TResult>(state, clientAsyncOperationFactory, configureAwait);
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