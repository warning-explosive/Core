namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// ActionExecutionInfo
    /// </summary>
    /// <typeparam name="TState">TState type-argument</typeparam>
    public class ActionExecutionInfo<TState>
    {
        private static readonly Action<Exception> EmptyExceptionHandler = _ => { };

        private readonly TState _state;
        private readonly Action<TState> _clientAction;
        private readonly IDictionary<Type, Action<Exception>> _exceptionHandlers;

        private Action? _finallyAction;

        /// <summary> .ctor </summary>
        /// <param name="state">State</param>
        /// <param name="clientAction">Client action</param>
        public ActionExecutionInfo(TState state, Action<TState> clientAction)
        {
            _state = state;
            _clientAction = clientAction;
            _exceptionHandlers = new Dictionary<Type, Action<Exception>>();
        }

        /// <summary>
        /// Catch block
        /// Catch exception of TException type
        /// </summary>
        /// <param name="exceptionHandler">Exception handler</param>
        /// <typeparam name="TException">Real exception type-argument</typeparam>
        /// <returns>ActionExecutionInfo</returns>
        public ActionExecutionInfo<TState> Catch<TException>(Action<Exception>? exceptionHandler = null)
        {
            _exceptionHandlers[typeof(TException)] = exceptionHandler ?? EmptyExceptionHandler;

            return this;
        }

        /// <summary>
        /// Finally block
        /// </summary>
        /// <param name="finallyAction">Finally action</param>
        /// <returns>ActionExecutionInfo</returns>
        public ActionExecutionInfo<TState> Finally(Action finallyAction)
        {
            _finallyAction = finallyAction;

            return this;
        }

        /// <summary>
        /// Invoke client's action
        /// </summary>
        public void Invoke()
        {
            try
            {
                _clientAction.Invoke(_state);
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
            }
            finally
            {
                _finallyAction?.Invoke();
            }
        }
    }
}