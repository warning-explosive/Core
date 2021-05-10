namespace SpaceEngineers.Core.DataAccess.Orm.Internals
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using AutoWiring.Api.Abstractions;

    /// <summary>
    /// IAsyncQueryProvider
    /// </summary>
    /// <typeparam name="T">T type-argument</typeparam>
    internal interface IAsyncQueryProvider<T> : IResolvable
    {
        /// <summary>
        /// Creates linq query object
        /// </summary>
        /// <param name="expression">Linq query expression</param>
        /// <returns>Linq query object</returns>
        IQueryable<T> CreateQuery(Expression expression);

        /// <summary>
        /// Execute linq query asynchronously
        /// </summary>
        /// <param name="expression">Linq query expression</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing execute operation</returns>
        IAsyncEnumerable<T> ExecuteAsync(Expression expression, CancellationToken token);

        /// <summary>
        /// Converts to IQueryProvider
        /// </summary>
        /// <returns>IQueryProvider</returns>
        IQueryProvider AsQueryProvider();
    }
}