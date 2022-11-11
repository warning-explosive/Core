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
        /// <param name="databaseImplementation">IDatabaseImplementation</param>
        /// <param name="commandText">Command text</param>
        /// <typeparam name="TResult">TResult type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static Func<Exception, CancellationToken, Task<TResult>> Handle<TResult>(
            this IDatabaseImplementation databaseImplementation, string commandText)
        {
            return (exception, _) =>
            {
                databaseImplementation.Handle(new DatabaseCommandExecutionException(commandText, exception));
                return Task.FromResult<TResult>(default!);
            };
        }
    }
}