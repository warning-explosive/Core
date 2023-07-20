namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Model
{
    using System;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model.Attributes;

    /// <summary>
    /// FunctionView
    /// </summary>
    [Schema(nameof(Migrations))]
    [Index(nameof(Schema), nameof(Function), Unique = true)]
    public record FunctionView : BaseDatabaseEntity<Guid>
    {
        /// <summary> .cctor </summary>
        /// <param name="primaryKey">Primary key</param>
        /// <param name="schema">Schema</param>
        /// <param name="function">Function</param>
        /// <param name="definition">Definition</param>
        public FunctionView(
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
        /// Function
        /// </summary>
        public string Function { get; set; }

        /// <summary>
        /// Definition
        /// </summary>
        public string Definition { get; set; }
    }
}