namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// AsyncOperationExecutionInfo
    /// </summary>
    public class AsyncOperationExecutionInfo
    {
        private static readonly Func<Exception, Task> EmptyExceptionHandler =
            _ => Task.CompletedTask;

        private readonly Func<Task> _clientAsyncOperationFactory;
        private readonly bool _configureAwait;
        private readonly IDictionary<Type, Func<Exception, Task>> _exceptionHandlers;

        private Func<Task>? _finallyAction;

        /// <summary> .cctor </summary>
        /// <param name="clientAsyncOperationFactory">Client async operation factory</param>
        /// <param name="configureAwait">Configure await option</param>
        public AsyncOperationExecutionInfo(Func<Task> clientAsyncOperationFactory, bool configureAwait = false)
        {
            _clientAsyncOperationFactory = clientAsyncOperationFactory;
            _configureAwait = configureAwait;
            _exceptionHandlers = new Dictionary<Type, Func<Exception, Task>>();
        }

        /// <summary>
        /// Async catch block
        /// Catch exception of TException type
        /// </summary>
        /// <param name="exceptionHandler">Async exception handler</param>
        /// <typeparam name="TException">Real exception type-argument</typeparam>
        /// <returns>AsyncOperationExecutionInfo</returns>
        public AsyncOperationExecutionInfo Catch<TException>(Func<Exception, Task>? exceptionHandler = null)
        {
            _exceptionHandlers[typeof(TException)] = exceptionHandler ?? EmptyExceptionHandler;

            return this;
        }

        /// <summary>
        /// Async finally block
        /// </summary>
        /// <param name="finallyActionFactory">Finally action factory</param>
        /// <returns>AsyncOperationExecutionInfo</returns>
        public AsyncOperationExecutionInfo Finally(Func<Task> finallyActionFactory)
        {
            _finallyAction = finallyActionFactory;

            return this;
        }

        /// <summary>
        /// Invoke client's async operation
        /// </summary>
        /// <param name="fallbackExceptionHandler">Async fallback exception handler</param>
        /// <returns>Ongoing client async action wrapped with error handling</returns>
        public async Task Invoke(Func<Exception, Task>? fallbackExceptionHandler = null)
        {
            if (fallbackExceptionHandler != null)
            {
                _exceptionHandlers[typeof(Exception)] = fallbackExceptionHandler;
            }

            try
            {
                await _clientAsyncOperationFactory.Invoke().ConfigureAwait(_configureAwait);
            }
            catch (Exception ex) when (ExecutionExtensions.CanBeCaught(ex.RealException()))
            {
                var realException = ex.RealException();
                var handled = false;

                foreach (var pair in _exceptionHandlers)
                {
                    if (pair.Key.IsInstanceOfType(realException))
                    {
                        await pair.Value.Invoke(realException).ConfigureAwait(_configureAwait);
                        handled = true;
                        break;
                    }
                }

                if (!handled)
                {
                    throw realException;
                }
            }
            finally
            {
                if (_finallyAction != null)
                {
                    await _finallyAction.Invoke().ConfigureAwait(_configureAwait);
                }
            }
        }
    }
}