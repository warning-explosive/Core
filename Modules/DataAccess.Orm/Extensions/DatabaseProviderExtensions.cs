namespace SpaceEngineers.Core.DataAccess.Orm.Extensions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Exceptions;
    using Connection;

    /// <summary>
    /// DatabaseProviderExtensions
    /// </summary>
    public static class DatabaseProviderExtensions
    {
        /// <summary>
        /// Handle
        /// </summary>
        /// <param name="databaseProvider">IDatabaseImplementation</param>
        /// <param name="commandText">Command text</param>
        /// <typeparam name="TResult">TResult type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static Func<Exception, CancellationToken, Task<TResult>> Handle<TResult>(
            this IDatabaseImplementation databaseProvider, string commandText)
        {
            return (exception, _) =>
            {
                databaseProvider.Handle(new DatabaseCommandExecutionException(commandText, exception));
                return Task.FromResult<TResult>(default!);
            };
        }
    }
}