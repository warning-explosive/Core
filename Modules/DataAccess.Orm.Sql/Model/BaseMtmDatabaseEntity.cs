namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using Api.Model;

    /// <summary>
    /// BaseMtmDatabaseEntity
    /// </summary>
    /// <typeparam name="TLeftKey">TLeftKey type-argument</typeparam>
    /// <typeparam name="TRightKey">TRightKey type-argument</typeparam>
    [Index(nameof(Left), nameof(Right), Unique = true)]
    public abstract class BaseMtmDatabaseEntity<TLeftKey, TRightKey>
        where TLeftKey : notnull
        where TRightKey : notnull
    {
        /// <summary> .cctor </summary>
        private BaseMtmDatabaseEntity()
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
        /// Left
        /// </summary>
        public TLeftKey Left { get; private init; }

        /// <summary>
        /// Right
        /// </summary>
        public TRightKey Right { get; private init; }
    }
}