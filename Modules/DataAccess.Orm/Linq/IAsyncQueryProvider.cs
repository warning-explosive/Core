namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System;
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
        /// Expression executed
        /// </summary>
        event EventHandler<ExecutedExpressionEventArgs>? ExpressionExecuted;

        /// <summary>
        /// Executes linq query asynchronously
        /// </summary>
        /// <param name="expression">Linq query expression</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task<long> ExecuteNonQueryAsync(Expression expression, CancellationToken token);

        /// <summary>
        /// Executes scalar linq query asynchronously
        /// </summary>
        /// <param name="expression">Linq query expression</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        Task<T> ExecuteScalarAsync<T>(Expression expression, CancellationToken token);

        /// <summary>
        /// Executes linq query asynchronously
        /// </summary>
        /// <param name="expression">Linq query expression</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        IAsyncEnumerable<T> ExecuteAsync<T>(Expression expression, CancellationToken token);
    }
}