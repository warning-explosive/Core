namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Action execution info
    /// </summary>
    public class ActionExecutionInfo
    {
        private static readonly Action<Exception> EmptyExceptionHandler =
            _ => { };

        private readonly Action _clientAction;
        private readonly IDictionary<Type, Action<Exception>> _exceptionHandlers;

        private Action? _finallyAction;

        /// <summary> .ctor </summary>
        /// <param name="clientAction">Client action</param>
        public ActionExecutionInfo(Action clientAction)
        {
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
        public ActionExecutionInfo Catch<TException>(Action<Exception>? exceptionHandler = null)
        {
            _exceptionHandlers[typeof(TException)] = exceptionHandler ?? EmptyExceptionHandler;

            return this;
        }

        /// <summary>
        /// Finally block
        /// </summary>
        /// <param name="finallyAction">Finally action</param>
        /// <returns>ActionExecutionInfo</returns>
        public ActionExecutionInfo Finally(Action finallyAction)
        {
            _finallyAction = finallyAction;

            return this;
        }

        /// <summary>
        /// Invoke client's action
        /// </summary>
        /// <param name="fallbackExceptionHandler">Fallback exception handler</param>
        public void Invoke(Action<Exception>? fallbackExceptionHandler = null)
        {
            if (fallbackExceptionHandler != null)
            {
                _exceptionHandlers[typeof(Exception)] = fallbackExceptionHandler;
            }

            try
            {
                _clientAction.Invoke();
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
                    realException.Rethrow();
                    throw realException;
                }
            }
            finally
            {
                _finallyAction?.Invoke();
            }
        }
    }
}