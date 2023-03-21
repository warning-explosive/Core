namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    using System;
    using Sql.Model;
    using Sql.Model.Attributes;

    /// <summary>
    /// DatabaseFunction
    /// </summary>
    [Schema(nameof(Migrations))]
    [Index(nameof(Schema), nameof(Function), Unique = true)]
    public record DatabaseFunction : BaseSqlView<Guid>
    {
        /// <summary> .cctor </summary>
        /// <param name="primaryKey">Primary key</param>
        /// <param name="schema">Schema</param>
        /// <param name="function">Function</param>
        /// <param name="definition">Definition</param>
        public DatabaseFunction(
            Guid primaryKey,
            string schema,
            string function,
            string definition)
            : base(primaryKey)
        {
            Schema = schema;
            Function = function;
            Definition = definition;
        }

        /// <summary>
        /// Schema
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Function { get; set; }

        /// <summary>
        /// Definition
        /// </summary>
        public string Definition { get; set; }
    }
}