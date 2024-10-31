namespace SpaceEngineers.Core.Basics;

using System;
using System.Linq;
using System.Threading.Tasks;
using Delegates;

public static class ExecutionExtensions
{
    private static readonly Type[] ExceptionTypesForSkip = new[]
    {
        typeof(StackOverflowException),
        typeof(OutOfMemoryException),
        typeof(OperationCanceledException),
        typeof(AccessViolationException)
    };

    public static StatelessActionExecutionInfo Try(
        this Action clientAction)
    {
        return new StatelessActionExecutionInfo(clientAction);
    }

    public static ActionExecutionInfo<TState> Try<TState>(
        this Action<TState> clientAction,
        TState state)
    {
        return new ActionExecutionInfo<TState>(state, clientAction);
    }

    public static StatelessFunctionExecutionInfo<TResult> Try<TResult>(
        this Func<TResult> clientFunction)
    {
        return new StatelessFunctionExecutionInfo<TResult>(clientFunction);
    }

    public static FunctionExecutionInfo<TState, TResult> Try<TState, TResult>(
        this Func<TState, TResult> clientFunction,
        TState state)
    {
        return new FunctionExecutionInfo<TState, TResult>(state, clientFunction);
    }

    public static AsyncOperationExecutionInfo TryAsync(
        this Task task,
        bool configureAwait = false)
    {
        return new AsyncOperationExecutionInfo(task, configureAwait);
    }

    public static AsyncOperationExecutionInfo<TResult> TryAsync<TResult>(
        this Task<TResult> task,
        bool configureAwait = false)
    {
        return new AsyncOperationExecutionInfo<TResult>(task, configureAwait);
    }

    internal static bool CanBeCaught(Exception exception)
    {
        return !ExceptionTypesForSkip.Contains(exception.GetType());
    }
}