namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// StatelessAsyncOperationExecutionInfo
    /// </summary>
    public class StatelessAsyncOperationExecutionInfo
    {
        private static readonly Func<Exception, CancellationToken, Task> EmptyExceptionHandler = (_, _) => Task.CompletedTask;

        private readonly Func<CancellationToken, Task> _clientAsyncOperationFactory;
        private readonly bool _configureAwait;
        private readonly IDictionary<Type, Func<Exception, CancellationToken, Task>> _exceptionHandlers;

        private Func<CancellationToken, Task>? _finallyAction;

        /// <summary> .cctor </summary>
        /// <param name="clientAsyncOperationFactory">Client async operation factory</param>
        /// <param name="configureAwait">Configure await option</param>
        public StatelessAsyncOperationExecutionInfo(
            Func<CancellationToken, Task> clientAsyncOperationFactory,
            bool configureAwait = false)
        {
            _clientAsyncOperationFactory = clientAsyncOperationFactory;
            _configureAwait = configureAwait;
            _exceptionHandlers = new Dictionary<Type, Func<Exception, CancellationToken, Task>>();
        }

        /// <summary>
        /// Async catch block
        /// Catch exception of TException type
        /// </summary>
        /// <param name="exceptionHandler">Async exception handler</param>
        /// <typeparam name="TException">Real exception type-argument</typeparam>
        /// <returns>StatelessAsyncOperationExecutionInfo</returns>
        public StatelessAsyncOperationExecutionInfo Catch<TException>(Func<Exception, CancellationToken, Task>? exceptionHandler = null)
        {
            _exceptionHandlers[typeof(TException)] = exceptionHandler ?? EmptyExceptionHandler;

            return this;
        }

        /// <summary>
        /// Async finally block
        /// </summary>
        /// <param name="finallyActionFactory">Finally action factory</param>
        /// <returns>StatelessAsyncOperationExecutionInfo</returns>
        public StatelessAsyncOperationExecutionInfo Finally(Func<CancellationToken, Task> finallyActionFactory)
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
                await _clientAsyncOperationFactory.Invoke(token).ConfigureAwait(_configureAwait);
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
}