namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    /// <summary>
    /// BaseSqlView
    /// </summary>
    /// <typeparam name="TKey">TKey type-argument</typeparam>
    public abstract record BaseSqlView<TKey> : ISqlView<TKey>
        where TKey : notnull
    {
        /// <summary> .cctor </summary>
        /// <param name="primaryKey">Primary key</param>
        protected BaseSqlView(TKey primaryKey)
        {
            PrimaryKey = primaryKey;
        }

        /// <inheritdoc />
        public TKey PrimaryKey { get; internal init; }

        object IUniqueIdentified.PrimaryKey => PrimaryKey;
    }
}