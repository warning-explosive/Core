namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Abstractions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// IAsyncQueryProvider
    /// </summary>
    internal interface IAsyncQueryProvider : IQueryProvider, IResolvable
    {
        /// <summary>
        /// Execute linq query asynchronously
        /// </summary>
        /// <param name="expression">Linq query expression</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing execute operation</returns>
        IAsyncEnumerable<T> ExecuteAsync<T>(Expression expression, CancellationToken token);
    }
}