namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    using System;
    using Sql.Model;
    using Sql.Model.Attributes;

    /// <summary>
    /// DatabaseView
    /// </summary>
    [Schema(nameof(Migrations))]
    [Index(nameof(Schema), nameof(View), Unique = true)]
    public record DatabaseView : BaseSqlView<Guid>
    {
        /// <summary> .cctor </summary>
        /// <param name="primaryKey">Primary key</param>
        /// <param name="schema">Schema</param>
        /// <param name="view">View</param>
        /// <param name="query">Query</param>
        public DatabaseView(
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