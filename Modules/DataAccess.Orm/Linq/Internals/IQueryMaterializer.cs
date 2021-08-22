namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Internals
{
    using System.Collections.Generic;
    using System.Threading;
    using Abstractions;
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// IQueryMaterializer
    /// </summary>
    /// <typeparam name="TQuery">TQuery type-argument</typeparam>
    /// <typeparam name="TItem">TItem type-argument</typeparam>
    internal interface IQueryMaterializer<in TQuery, out TItem> : IResolvable
        where TQuery : IQuery
    {
        /// <summary>
        /// Materializes query
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing materialization operation</returns>
        IAsyncEnumerable<TItem> Materialize(TQuery query, CancellationToken token);
    }
}