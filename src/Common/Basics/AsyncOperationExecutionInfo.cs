namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// AsyncOperationExecutionInfo
    /// </summary>
    public class AsyncOperationExecutionInfo
    {
        private static readonly Func<Exception, CancellationToken, Task> EmptyExceptionHandler = (_, _) => Task.CompletedTask;

        private readonly Task _asyncOperation;
        private readonly bool _configureAwait;
        private readonly IDictionary<Type, Func<Exception, CancellationToken, Task>> _exceptionHandlers;

        private Func<CancellationToken, Task>? _finallyAction;

        /// <summary> .cctor </summary>
        /// <param name="asyncOperation">Async operation</param>
        /// <param name="configureAwait">Configure await option</param>
        public AsyncOperationExecutionInfo(
            Task asyncOperation,
            bool configureAwait = false)
        {
            _asyncOperation = asyncOperation;
            _configureAwait = configureAwait;
            _exceptionHandlers = new Dictionary<Type, Func<Exception, CancellationToken, Task>>();
        }

        /// <summary>
        /// Async catch block
        /// Catch exception of TException type
        /// </summary>
        /// <param name="exceptionHandler">Async exception handler</param>
        /// <typeparam name="TException">Real exception type-argument</typeparam>
        /// <returns>AsyncOperationExecutionInfo</returns>
        public AsyncOperationExecutionInfo Catch<TException>(Func<Exception, CancellationToken, Task>? exceptionHandler = null)
        {
            _exceptionHandlers[typeof(TException)] = exceptionHandler ?? EmptyExceptionHandler;

            return this;
        }

        /// <summary>
        /// Async finally block
        /// </summary>
        /// <param name="finallyActionFactory">Finally action factory</param>
        /// <returns>AsyncOperationExecutionInfo</returns>
        public AsyncOperationExecutionInfo Finally(Func<CancellationToken, Task> finallyActionFactory)
        {
            _finallyAction = finallyActionFactory;

            return this;
        }

        /// <summary>
        /// Invoke client's async operation
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing client async action wrapped with error handling</returns>
        public async Task Invoke(CancellationToken token)
        {
            try
            {
                await _asyncOperation.ConfigureAwait(_configureAwait);
            }
            catch (Exception ex) when (ExecutionExtensions.CanBeCaught(ex.RealException()))
            {
                var realException = ex.RealException();
                var handled = false;

                foreach (var pair in _exceptionHandlers)
                {
                    if (pair.Key.IsInstanceOfType(realException))
                    {
                        await pair.Value.Invoke(realException, token).ConfigureAwait(_configureAwait);
                        handled = true;
                        break;
                    }
                }

                if (!handled)
                {
                    throw realException.Rethrow();
                }
            }
            finally
            {
                if (_finallyAction != null)
                {
                    await _finallyAction.Invoke(token).ConfigureAwait(_configureAwait);
                }
            }
        }
    }

    /// <summary>
    /// AsyncOperationExecutionInfo
    /// </summary>
    /// <typeparam name="TResult">TResult type-argument</typeparam>
    [SuppressMessage("Analysis", "SA1402", Justification = "generic data type with the same name")]
    public class AsyncOperationExecutionInfo<TResult>
    {
        private static readonly Func<Exception, CancellationToken, Task> EmptyExceptionHandler = (_, _) => Task.CompletedTask;

        private readonly Task<TResult> _asyncOperation;
        private readonly bool _configureAwait;
        private readonly IDictionary<Type, Func<Exception, CancellationToken, Task>> _exceptionHandlers;

        private Func<CancellationToken, Task>? _finallyAction;

        /// <summary> .cctor </summary>
        /// <param name="asyncOperation">Client async operation factory</param>
        /// <param name="configureAwait">Configure await option</param>
        public AsyncOperationExecutionInfo(
            Task<TResult> asyncOperation,
            bool configureAwait = false)
        {
            _asyncOperation = asyncOperation;
            _configureAwait = configureAwait;
            _exceptionHandlers = new Dictionary<Type, Func<Exception, CancellationToken, Task>>();
        }

        /// <summary>
        /// Async catch block
        /// Catch exception of TException type
        /// </summary>
        /// <param name="exceptionHandler">Async exception handler</param>
        /// <typeparam name="TException">Real exception type-argument</typeparam>
        /// <returns>AsyncOperationExecutionInfo</returns>
        public AsyncOperationExecutionInfo<TResult> Catch<TException>(Func<Exception, CancellationToken, Task>? exceptionHandler = null)
        {
            _exceptionHandlers[typeof(TException)] = exceptionHandler ?? EmptyExceptionHandler;

            return this;
        }

        /// <summary>
        /// Async finally block
        /// </summary>
        /// <param name="finallyActionFactory">Finally action factory</param>
        /// <returns>AsyncOperationExecutionInfo</returns>
        public AsyncOperationExecutionInfo<TResult> Finally(Func<CancellationToken, Task> finallyActionFactory)
        {
            _finallyAction = finallyActionFactory;

            return this;
        }

        /// <summary>
        /// Invoke client's async operation
        /// </summary>
        /// <param name="exceptionResultFactory">Creates result from handled exception</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing client async action wrapped with error handling</returns>
        public async Task<TResult> Invoke(
            Func<Exception, TResult> exceptionResultFactory,
            CancellationToken token)
        {
            try
            {
                return await _asyncOperation.ConfigureAwait(_configureAwait);
            }
            catch (Exception ex) when (ExecutionExtensions.CanBeCaught(ex.RealException()))
            {
                var realException = ex.RealException();
                var handled = false;

                foreach (var pair in _exceptionHandlers)
                {
                    if (pair.Key.IsInstanceOfType(realException))
                    {
                        await pair.Value.Invoke(realException, token).ConfigureAwait(_configureAwait);
                        handled = true;
                        break;
                    }
                }

                if (!handled)
                {
                    throw realException.Rethrow();
                }

                return exceptionResultFactory(realException);
            }
            finally
            {
                if (_finallyAction != null)
                {
                    await _finallyAction.Invoke(token).ConfigureAwait(_configureAwait);
                }
            }
        }
    }
}