namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Transaction
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using CompositionRoot;

    /// <summary>
    /// Query invocation extensions
    /// </summary>
    public static class QueryInvocationExtensions
    {
        /// <summary>
        /// Invokes producer within database transaction
        /// </summary>
        /// <param name="dependencyContainer">IDependencyContainer</param>
        /// <param name="commit">Commit transaction or not</param>
        /// <param name="producer">Producer</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        public static async Task InvokeWithinTransaction(
            this IDependencyContainer dependencyContainer,
            bool commit,
            Func<IAdvancedDatabaseTransaction, CancellationToken, Task> producer,
            CancellationToken token)
        {
            await using (dependencyContainer.OpenScopeAsync().ConfigureAwait(false))
            {
                var transaction = dependencyContainer.Resolve<IAdvancedDatabaseTransaction>();

                await using (await transaction.OpenScope(commit, token).ConfigureAwait(false))
                {
                    await producer(transaction, token).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Invokes producer within database transaction
        /// </summary>
        /// <param name="dependencyContainer">IDependencyContainer</param>
        /// <param name="commit">Commit transaction or not</param>
        /// <param name="producer">Producer</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TResult">TResult type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static async Task<TResult> InvokeWithinTransaction<TResult>(
            this IDependencyContainer dependencyContainer,
            bool commit,
            Func<IAdvancedDatabaseTransaction, CancellationToken, Task<TResult>> producer,
            CancellationToken token)
        {
            await using (dependencyContainer.OpenScopeAsync().ConfigureAwait(false))
            {
                var transaction = dependencyContainer.Resolve<IAdvancedDatabaseTransaction>();

                await using (await transaction.OpenScope(commit, token).ConfigureAwait(false))
                {
                    return await producer(transaction, token).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Invokes producer within database transaction
        /// </summary>
        /// <param name="dependencyContainer">IDependencyContainer</param>
        /// <param name="commit">Commit transaction or not</param>
        /// <param name="state">State</param>
        /// <param name="producer">Producer</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TState">TState type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static async Task InvokeWithinTransaction<TState>(
            this IDependencyContainer dependencyContainer,
            bool commit,
            TState state,
            Func<IAdvancedDatabaseTransaction, TState, CancellationToken, Task> producer,
            CancellationToken token)
        {
            await using (dependencyContainer.OpenScopeAsync().ConfigureAwait(false))
            {
                var transaction = dependencyContainer.Resolve<IAdvancedDatabaseTransaction>();

                await using (await transaction.OpenScope(commit, token).ConfigureAwait(false))
                {
                    await producer(transaction, state, token).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Invokes producer within database transaction
        /// </summary>
        /// <param name="dependencyContainer">IDependencyContainer</param>
        /// <param name="commit">Commit transaction or not</param>
        /// <param name="state">State</param>
        /// <param name="producer">Producer</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TResult">TResult type-argument</typeparam>
        /// <typeparam name="TState">TState type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static async Task<TResult> InvokeWithinTransaction<TResult, TState>(
            this IDependencyContainer dependencyContainer,
            bool commit,
            TState state,
            Func<IAdvancedDatabaseTransaction, TState, CancellationToken, Task<TResult>> producer,
            CancellationToken token)
        {
            await using (dependencyContainer.OpenScopeAsync().ConfigureAwait(false))
            {
                var transaction = dependencyContainer.Resolve<IAdvancedDatabaseTransaction>();

                await using (await transaction.OpenScope(commit, token).ConfigureAwait(false))
                {
                    return await producer(transaction, state, token).ConfigureAwait(false);
                }
            }
        }
    }
}