namespace SpaceEngineers.Core.DataAccess.Api.Persisting
{
    using System;
    using System.Linq.Expressions;
    using Model;

    /// <summary>
    /// UpdateInfo
    /// </summary>
    /// <typeparam name="TEntity">TEntity type-argument</typeparam>
    public class UpdateInfo<TEntity>
        where TEntity : IDatabaseEntity
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