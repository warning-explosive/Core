namespace SpaceEngineers.Core.DataAccess.Contract
{
    using System;
    using System.Linq;
    using GenericDomain.Abstractions;

    /// <summary>
    /// Query builder entry point
    /// </summary>
    public static class Query
    {
        /// <summary>
        /// Query all items from the database
        /// </summary>
        /// <typeparam name="TAggregate">TAggregate type-argument</typeparam>
        /// <returns>Query object</returns>
        public static IQueryable<TAggregate> All<TAggregate>()
            where TAggregate : class, IAggregate
        {
            throw new NotImplementedException();
        }
    }
}