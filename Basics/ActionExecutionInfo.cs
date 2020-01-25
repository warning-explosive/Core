namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Action execution info
    /// </summary>
    public class ActionExecutionInfo
    {
        private readonly Action _clientAction;

        private readonly IDictionary<Type, Action<Exception>> _exceptionHandlers = new Dictionary<Type, Action<Exception>>();

        private Action? _finallyAction;

        /// <summary> .ctor </summary>
        /// <param name="clientAction">Client action</param>
        public ActionExecutionInfo(Action clientAction)
        {
            _clientAction = clientAction;
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
            _exceptionHandlers[typeof(TException)] = exceptionHandler ?? (ex => { });

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
        /// Invoke client action
        /// </summary>
        public void Invoke()
        {
            try
            {
                _clientAction.Invoke();
            }
            catch (Exception ex) when (ExecutionExtensions.CanBeCatched(ex.RealException()))
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
                    throw;
                }
            }
            finally
            {
                _finallyAction?.Invoke();
            }
        }
    }
}