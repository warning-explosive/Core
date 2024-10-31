namespace SpaceEngineers.Core.Basics.Delegates;

using System;
using System.Collections.Generic;

public class ActionExecutionInfo<TState>
{
    private static readonly Action<Exception> EmptyExceptionHandler = _ => { };

    private readonly TState _state;
    private readonly Action<TState> _clientAction;
    private readonly IDictionary<Type, Action<Exception>> _exceptionHandlers;

    private Action? _finallyAction;

    public ActionExecutionInfo(TState state, Action<TState> clientAction)
    {
        _state = state;
        _clientAction = clientAction;
        _exceptionHandlers = new Dictionary<Type, Action<Exception>>();
    }

    public ActionExecutionInfo<TState> Catch<TException>(Action<Exception>? exceptionHandler = null)
    {
        _exceptionHandlers[typeof(TException)] = exceptionHandler ?? EmptyExceptionHandler;

        return this;
    }

    public ActionExecutionInfo<TState> Finally(Action finallyAction)
    {
        _finallyAction = finallyAction;

        return this;
    }

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