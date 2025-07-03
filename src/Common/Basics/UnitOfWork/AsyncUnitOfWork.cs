namespace SpaceEngineers.Core.Basics.UnitOfWork;

using System;
using System.Threading;
using System.Threading.Tasks;
using SynchronizationPrimitives;

public abstract class AsyncUnitOfWork<TContext> : IAsyncUnitOfWork<TContext>
{
    private readonly Exclusive _exclusive = new();

    public async Task ExecuteInTransaction(
        TContext context,
        Func<TContext, CancellationToken, Task> producer,
        bool saveChanges,
        CancellationToken token)
    {
        using (await _exclusive.Run(token).ConfigureAwait(false))
        {
            var (behavior, startError) = await StartTransactionUnsafe(context, token)
                .TryAsync()
                .Catch<Exception>()
                .Invoke(StartExceptionResult, token)
                .ConfigureAwait(false);

            if (startError != null)
            {
                throw startError.Rethrow();
            }

            if (behavior is EnUnitOfWorkBehavior.DoNotRun)
            {
                return;
            }

            Exception? executionError = null;

            if (behavior is not EnUnitOfWorkBehavior.SkipProducer)
            {
                executionError = await ExecuteProducerUnsafe(context, producer, token)
                    .TryAsync()
                    .Catch<Exception>()
                    .Invoke(ExceptionResult, token)
                    .ConfigureAwait(false);
            }

            var finishError = await FinishTransactionUnsafe(context, saveChanges, executionError, token)
                .TryAsync()
                .Catch<Exception>()
                .Invoke(ExceptionResult, token)
                .ConfigureAwait(false);

            var error = executionError ?? finishError;

            if (error != null)
            {
                throw error.Rethrow();
            }
        }
    }

    protected virtual Task<EnUnitOfWorkBehavior> Start(TContext context, CancellationToken token)
    {
        return Task.FromResult(EnUnitOfWorkBehavior.Regular);
    }

    protected virtual Task Rollback(TContext context, Exception? exception, CancellationToken token)
    {
        return Task.CompletedTask;
    }

    protected virtual Task Commit(TContext context, CancellationToken token)
    {
        return Task.CompletedTask;
    }

    private async Task<(EnUnitOfWorkBehavior, Exception?)> StartTransactionUnsafe(TContext context, CancellationToken token)
    {
        var behavior = await Start(context, token).ConfigureAwait(false);
        return (behavior, null);
    }

    private static async Task<Exception?> ExecuteProducerUnsafe(TContext context, Func<TContext, CancellationToken, Task> producer, CancellationToken token)
    {
        await producer.Invoke(context, token).ConfigureAwait(false);

        return null;
    }

    private async Task<Exception?> FinishTransactionUnsafe(TContext context, bool saveChanges, Exception? exception, CancellationToken token)
    {
        var finishOperation = saveChanges && exception == null
            ? Commit(context, token)
            : Rollback(context, exception, token);

        await finishOperation.ConfigureAwait(false);

        return null;
    }

    private static (EnUnitOfWorkBehavior, Exception?) StartExceptionResult(Exception exception)
    {
        return (EnUnitOfWorkBehavior.DoNotRun, (Exception?)exception);
    }

    private static Exception? ExceptionResult(Exception exception)
    {
        return exception;
    }
}