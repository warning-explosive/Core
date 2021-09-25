namespace SpaceEngineers.Core.DataAccess.Api.Exceptions
{
    using System;
    using Model;

    /// <summary>
    /// EntityNotFoundException
    /// </summary>
    /// <typeparam name="TEntity">TEntity type-argument</typeparam>
    /// <typeparam name="TKey">TKey type-argument</typeparam>
    public class EntityNotFoundException<TEntity, TKey> : Exception
        where TEntity : IUniqueIdentified<TKey>
    {
        /// <summary> .cctor </summary>
        /// <param name="primaryKey">Primary key</param>
        public EntityNotFoundException(TKey primaryKey)
            : base($"Entity {typeof(TEntity).Name} with PK {primaryKey} wasn't found")
        {
            PrimaryKey = primaryKey;
        }

        /// <summary>
        /// Primary key
        /// </summary>
        public TKey PrimaryKey { get; }
    }
}