namespace SpaceEngineers.Core.DataAccess.Orm.Extensions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Connection;

    internal static class DatabaseProviderExtensions
    {
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