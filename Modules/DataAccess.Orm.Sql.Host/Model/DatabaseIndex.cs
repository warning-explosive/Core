namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    using System;
    using Sql.Model;
    using Sql.Model.Attributes;

    /// <summary>
    /// DatabaseIndex
    /// </summary>
    [Schema(nameof(Migrations))]
    [Index(nameof(Schema), nameof(Table), nameof(Index), Unique = true)]
    public record DatabaseIndex : BaseSqlView<Guid>
    {
        /// <summary> .cctor </summary>
        /// <param name="primaryKey">Primary key</param>
        /// <param name="schema">Schema</param>
        /// <param name="table">Table</param>
        /// <param name="index">Index</param>
        /// <param name="definition">Definition</param>
        public DatabaseIndex(
            Guid primaryKey,
            string schema,
            string table,
            string index,
            string definition)
            : base(primaryKey)
        {
            Schema = schema;
            Table = table;
            Index = index;
            Definition = definition;
        }

        /// <summary>
        /// Schema
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// Table
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        /// Index
        /// </summary>
        public string Index { get; set; }

        /// <summary>
        /// Definition
        /// </summary>
        public string Definition { get; set; }
    }
}