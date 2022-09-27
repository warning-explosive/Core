namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    using System;
    using Api.Model;

    /// <summary>
    /// SqlView
    /// </summary>
    [Schema(nameof(DataAccess.Orm.Host.Migrations))]
    [Index(nameof(Schema), nameof(View), Unique = true)]
    public record SqlView : BaseDatabaseEntity<Guid>
    {
        /// <summary> .cctor </summary>
        /// <param name="primaryKey">Primary key</param>
        /// <param name="schema">Schema</param>
        /// <param name="view">View</param>
        /// <param name="query">Query</param>
        public SqlView(
            Guid primaryKey,
            string schema,
            string view,
            string query)
            : base(primaryKey)
        {
            Schema = schema;
            View = view;
            Query = query;
        }

        /// <summary>
        /// Schema
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// View
        /// </summary>
        public string View { get; set; }

        /// <summary>
        /// Query
        /// </summary>
        public string Query { get; set; }
    }
}