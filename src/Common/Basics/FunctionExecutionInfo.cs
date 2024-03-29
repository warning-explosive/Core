namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// FunctionExecutionInfo
    /// </summary>
    /// <typeparam name="TState">TState Type-argument</typeparam>
    /// <typeparam name="TResult">TResult Type-argument</typeparam>
    public class FunctionExecutionInfo<TState, TResult>
    {
        private static readonly Action<Exception> EmptyExceptionHandler = _ => { };

        private readonly TState _state;
        private readonly Func<TState, TResult> _clientFunction;
        private readonly IDictionary<Type, Action<Exception>> _exceptionHandlers;

        private Action? _finallyAction;

        /// <summary> .ctor </summary>
        /// <param name="state">State</param>
        /// <param name="clientFunction">Client function</param>
        public FunctionExecutionInfo(
            TState state,
            Func<TState, TResult> clientFunction)
        {
            _state = state;
            _clientFunction = clientFunction;
            _exceptionHandlers = new Dictionary<Type, Action<Exception>>();
        }

        /// <summary>
        /// Catch block
        /// Catch exception of TException type
        /// </summary>
        /// <param name="exceptionHandler">Exception handler</param>
        /// <typeparam name="TException">Real exception type-argument</typeparam>
        /// <returns>FunctionExecutionInfo</returns>
        public FunctionExecutionInfo<TState, TResult> Catch<TException>(Action<Exception>? exceptionHandler = null)
        {
            _exceptionHandlers[typeof(TException)] = exceptionHandler ?? EmptyExceptionHandler;

            return this;
        }

        /// <summary>
        /// Finally block
        /// </summary>
        /// <param name="finallyAction">Finally action</param>
        /// <returns>FunctionExecutionInfo</returns>
        public FunctionExecutionInfo<TState, TResult> Finally(Action finallyAction)
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
                return _clientFunction.Invoke(_state);
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