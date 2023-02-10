namespace SpaceEngineers.Core.DataAccess.Orm.Extensions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Exceptions;
    using Connection;

    /// <summary>
    /// DatabaseConnectionProviderExtensions
    /// </summary>
    public static class DatabaseConnectionProviderExtensions
    {
        /// <summary>
        /// Handle
        /// </summary>
        /// <param name="databaseConnectionProvider">IDatabaseConnectionProvider</param>
        /// <param name="commandText">Command text</param>
        /// <typeparam name="TResult">TResult type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static Func<Exception, CancellationToken, Task<TResult>> Handle<TResult>(
            this IDatabaseConnectionProvider databaseConnectionProvider, string commandText)
        {
            return (exception, _) =>
            {
                databaseConnectionProvider.Handle(new DatabaseCommandExecutionException(commandText, exception));
                return Task.FromResult<TResult>(default!);
            };
        }
    }
}