namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using Api.Model;
    using Api.Sql.Attributes;

    /// <summary>
    /// BaseMtmDatabaseEntity
    /// </summary>
    /// <typeparam name="TLeftKey">TLeftKey type-argument</typeparam>
    /// <typeparam name="TRightKey">TRightKey type-argument</typeparam>
    [Index(nameof(Left), nameof(Right), Unique = true)]
    public abstract class BaseMtmDatabaseEntity<TLeftKey, TRightKey> : IUniqueIdentified<object>
        where TLeftKey : notnull
        where TRightKey : notnull
    {
        /// <summary> .cctor </summary>
        protected BaseMtmDatabaseEntity()
            : this(default!, default!)
        {
        }

        /// <summary> .cctor </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        private BaseMtmDatabaseEntity(TLeftKey left, TRightKey right)
        {
            Left = left;
            Right = right;
        }

        /// <summary>
        /// Primary key
        /// </summary>
        public object PrimaryKey => HashCode.Combine(Left, Right);

        /// <summary>
        /// Left
        /// </summary>
        public TLeftKey Left { get; internal init; }

        /// <summary>
        /// Right
        /// </summary>
        public TRightKey Right { get; internal init; }
    }
}