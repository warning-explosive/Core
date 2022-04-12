namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// IAsyncQueryProvider
    /// </summary>
    public interface IAsyncQueryProvider : IQueryProvider
    {
        /// <summary>
        /// Execute scalar linq query asynchronously
        /// </summary>
        /// <param name="expression">Linq query expression</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        Task<T> ExecuteScalarAsync<T>(Expression expression, CancellationToken token);

        /// <summary>
        /// Execute linq query asynchronously
        /// </summary>
        /// <param name="expression">Linq query expression</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        IAsyncEnumerable<T> ExecuteAsync<T>(Expression expression, CancellationToken token);
    }
}