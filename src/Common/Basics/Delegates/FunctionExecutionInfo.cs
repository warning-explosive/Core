namespace SpaceEngineers.Core.Basics.Delegates;

using System;
using System.Collections.Generic;

public class FunctionExecutionInfo<TState, TResult>(TState state, Func<TState, TResult> clientFunction)
{
    private static readonly Action<Exception> EmptyExceptionHandler = _ => { };

    private readonly IDictionary<Type, Action<Exception>> _exceptionHandlers = new Dictionary<Type, Action<Exception>>();

    private Action? _finallyAction;

    public FunctionExecutionInfo<TState, TResult> Catch<TException>(Action<Exception>? exceptionHandler = null)
    {
        _exceptionHandlers[typeof(TException)] = exceptionHandler ?? EmptyExceptionHandler;

        return this;
    }

    public FunctionExecutionInfo<TState, TResult> Finally(Action finallyAction)
    {
        _finallyAction = finallyAction;

        return this;
    }

    public TResult Invoke(Func<Exception, TResult> exceptionResultFactory)
    {
        try
        {
            return clientFunction.Invoke(state);
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