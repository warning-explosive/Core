namespace SpaceEngineers.Core.Basics.Delegates;

using System;
using System.Collections.Generic;

public class StatelessActionExecutionInfo(Action clientAction)
{
    private static readonly Action<Exception> EmptyExceptionHandler = _ => { };

    private readonly IDictionary<Type, Action<Exception>> _exceptionHandlers = new Dictionary<Type, Action<Exception>>();

    private Action? _finallyAction;

    public StatelessActionExecutionInfo Catch<TException>(Action<Exception>? exceptionHandler = null)
    {
        _exceptionHandlers[typeof(TException)] = exceptionHandler ?? EmptyExceptionHandler;

        return this;
    }

    public StatelessActionExecutionInfo Finally(Action finallyAction)
    {
        _finallyAction = finallyAction;

        return this;
    }

    public void Invoke()
    {
        try
        {
            clientAction.Invoke();
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