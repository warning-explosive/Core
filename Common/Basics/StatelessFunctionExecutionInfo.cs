namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// StatelessFunctionExecutionInfo
    /// </summary>
    /// <typeparam name="TResult">Function TResult Type-argument</typeparam>
    public class StatelessFunctionExecutionInfo<TResult>
    {
        private static readonly Action<Exception> EmptyExceptionHandler = _ => { };

        private readonly Func<TResult> _clientFunction;
        private readonly IDictionary<Type, Action<Exception>> _exceptionHandlers;

        private Action? _finallyAction;

        /// <summary> .ctor </summary>
        /// <param name="clientFunction">Client function</param>
        public StatelessFunctionExecutionInfo(Func<TResult> clientFunction)
        {
            _clientFunction = clientFunction;
            _exceptionHandlers = new Dictionary<Type, Action<Exception>>();
        }

        /// <summary>
        /// Catch block
        /// Catch exception of TException type
        /// </summary>
        /// <param name="exceptionHandler">Exception handler</param>
        /// <typeparam name="TException">Real exception type-argument</typeparam>
        /// <returns>StatelessFunctionExecutionInfo</returns>
        public StatelessFunctionExecutionInfo<TResult> Catch<TException>(Action<Exception>? exceptionHandler = null)
        {
            _exceptionHandlers[typeof(TException)] = exceptionHandler ?? EmptyExceptionHandler;

            return this;
        }

        /// <summary>
        /// Finally block
        /// </summary>
        /// <param name="finallyAction">Finally action</param>
        /// <returns>StatelessFunctionExecutionInfo</returns>
        public StatelessFunctionExecutionInfo<TResult> Finally(Action finallyAction)
        {
            _finallyAction = finallyAction;

            return this;
        }

        /// <summary>
        /// Invoke client's function
        /// </summary>
        /// <param name="exceptionResultFactory">Creates result from handled exception</param>
        /// <returns>TResult</returns>
        public TResult Invoke(Func<Exception, TResult> exceptionResultFactory)
        {
            try
            {
                return _clientFunction.Invoke();
            }
            catch (Exception ex) when (ExecutionExtensions.CanBeCaught(ex.RealException()))
            {
                var realException = ex.RealException();
                var handled = false;

                foreach (var pair in _exceptionHandlers)
                {
                    if (pair.Key.IsInstanceOfType(realException))
                    {
                        pair.Value.Invoke(realException);
                        handled = true;
                        break;
                    }
                }

                if (!handled)
                {
                    throw realException.Rethrow();
                }

                return exceptionResultFactory.Invoke(realException);
            }
            finally
            {
                _finallyAction?.Invoke();
            }
        }
    }
}