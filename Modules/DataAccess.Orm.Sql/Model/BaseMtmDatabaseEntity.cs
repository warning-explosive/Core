namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using Api.Model;

    /// <summary>
    /// BaseMtmDatabaseEntity
    /// </summary>
    /// <typeparam name="TKey">TKey type-argument</typeparam>
    [Index(nameof(Left), nameof(Right), Unique = true)]
    public abstract class BaseMtmDatabaseEntity<TKey>
        where TKey : notnull
    {
        /// <summary> .cctor </summary>
        private BaseMtmDatabaseEntity()
            : this(default!, default!)
        {
        }

        /// <summary> .cctor </summary>
        /// <param name="left">Lefty</param>
        /// <param name="right">Right</param>
        private BaseMtmDatabaseEntity(TKey left, TKey right)
        {
            Left = left;
            Right = right;
        }

        /// <summary>
        /// Left
        /// </summary>
        public TKey Left { get; private init; }

        /// <summary>
        /// Right
        /// </summary>
        public TKey Right { get; private init; }
    }
}