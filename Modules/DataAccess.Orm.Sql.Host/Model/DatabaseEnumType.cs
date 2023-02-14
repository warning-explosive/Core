namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    using System;
    using Api.Sql;
    using Api.Sql.Attributes;

    /// <summary>
    /// DatabaseEnumType
    /// </summary>
    [Schema(nameof(Migrations))]
    [Index(nameof(Schema), nameof(Type), nameof(Value), Unique = true)]
    public record DatabaseEnumType : BaseSqlView<Guid>
    {
        /// <summary> .cctor </summary>
        /// <param name="primaryKey">Primary key</param>
        /// <param name="schema">Schema</param>
        /// <param name="type">Type</param>
        /// <param name="value">Value</param>
        public DatabaseEnumType(
            Guid primaryKey,
            string schema,
            string type,
            string value)
            : base(primaryKey)
        {
            Schema = schema;
            Type = type;
            Value = value;
        }

        /// <summary>
        /// Schema
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// Type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Value
        /// </summary>
        public string Value { get; set; }
    }
}