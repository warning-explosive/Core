namespace SpaceEngineers.Core.DataAccess.Api.Persisting
{
    using System;
    using System.Linq.Expressions;
    using Model;

    /// <summary>
    /// UpdateInfo
    /// </summary>
    /// <typeparam name="TEntity">TEntity type-argument</typeparam>
    /// <typeparam name="TKey">TKey type-argument</typeparam>
    public class UpdateInfo<TEntity, TKey>
        where TEntity : IDatabaseEntity<TKey>
        where TKey : notnull
    {
        /// <summary> .cctor </summary>
        /// <param name="accessor">Accessor</param>
        /// <param name="valueProducer">Value producer</param>
        public UpdateInfo(
            Expression<Func<TEntity, object?>> accessor,
            Expression<Func<TEntity, object?>> valueProducer)
        {
            Accessor = accessor;
            ValueProducer = valueProducer;
        }

        /// <summary>
        /// Accessor
        /// </summary>
        public Expression<Func<TEntity, object?>> Accessor { get; }

        /// <summary>
        /// ValueProducer
        /// </summary>
        public Expression<Func<TEntity, object?>> ValueProducer { get; }
    }
}