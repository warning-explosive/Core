namespace SpaceEngineers.Core.DataAccess.Orm.Extensions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Connection;

    /// <summary>
    /// DatabaseProviderExtensions
    /// </summary>
    public static class DatabaseProviderExtensions
    {
        /// <summary>
        /// Handle
        /// </summary>
        /// <param name="databaseProvider">IDatabaseProvider</param>
        /// <param name="commandText">Command text</param>
        /// <typeparam name="TResult">TResult type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static Func<Exception, CancellationToken, Task<TResult>> Handle<TResult>(
            this IDatabaseProvider databaseProvider, string commandText)
        {
            return (exception, _) =>
            {
                databaseProvider.Handle(commandText, exception);
                return Task.FromResult<TResult>(default!);
            };
        }
    }
}