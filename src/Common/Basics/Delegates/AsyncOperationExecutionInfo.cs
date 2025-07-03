namespace SpaceEngineers.Core.Basics.Delegates;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class AsyncOperationExecutionInfo(Task asyncOperation, bool configureAwait = false)
{
    private static readonly Func<Exception, CancellationToken, Task> EmptyExceptionHandler = (_, _) => Task.CompletedTask;

    private readonly IDictionary<Type, Func<Exception, CancellationToken, Task>> _exceptionHandlers = new Dictionary<Type, Func<Exception, CancellationToken, Task>>();

    private Func<CancellationToken, Task>? _finallyAction;

    public AsyncOperationExecutionInfo Catch<TException>(Func<Exception, CancellationToken, Task>? exceptionHandler = null)
    {
        _exceptionHandlers[typeof(TException)] = exceptionHandler ?? EmptyExceptionHandler;

        return this;
    }

    public AsyncOperationExecutionInfo Finally(Func<CancellationToken, Task> finallyActionFactory)
    {
        _finallyAction = finallyActionFactory;

        return this;
    }

    public async Task Invoke(CancellationToken token)
    {
        try
        {
            await asyncOperation.ConfigureAwait(configureAwait);
        }
        catch (Exception ex) when (ExecutionExtensions.CanBeCaught(ex.RealException()))
        {
            var realException = ex.RealException();
            var handled = false;

            foreach (var pair in _exceptionHandlers)
            {
                if (pair.Key.IsInstanceOfType(realException))
                {
                    await pair.Value.Invoke(realException, token).ConfigureAwait(configureAwait);
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
                await _finallyAction.Invoke(token).ConfigureAwait(configureAwait);
            }
        }
    }
}

public class AsyncOperationExecutionInfo<TResult>
{
    private static readonly Func<Exception, CancellationToken, Task> EmptyExceptionHandler = (_, _) => Task.CompletedTask;

    private readonly Task<TResult> _asyncOperation;
    private readonly bool _configureAwait;
    private readonly IDictionary<Type, Func<Exception, CancellationToken, Task>> _exceptionHandlers;

    private Func<CancellationToken, Task>? _finallyAction;

    public AsyncOperationExecutionInfo(
        Task<TResult> asyncOperation,
        bool configureAwait = false)
    {
        _asyncOperation = asyncOperation;
        _configureAwait = configureAwait;
        _exceptionHandlers = new Dictionary<Type, Func<Exception, CancellationToken, Task>>();
    }

    public AsyncOperationExecutionInfo<TResult> Catch<TException>(Func<Exception, CancellationToken, Task>? exceptionHandler = null)
    {
        _exceptionHandlers[typeof(TException)] = exceptionHandler ?? EmptyExceptionHandler;

        return this;
    }

    public AsyncOperationExecutionInfo<TResult> Finally(Func<CancellationToken, Task> finallyActionFactory)
    {
        _finallyAction = finallyActionFactory;

        return this;
    }

    public async Task<TResult> Invoke(
        Func<Exception, TResult> exceptionResultFactory,
        CancellationToken token)
    {
        try
        {
            return await _asyncOperation.ConfigureAwait(_configureAwait);
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

            return exceptionResultFactory(realException);
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