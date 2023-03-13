namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    using System;
    using Sql.Model;
    using Sql.Model.Attributes;

    /// <summary>
    /// DatabaseTrigger
    /// </summary>
    [Schema(nameof(Migrations))]
    [Index(nameof(Schema), nameof(Trigger), Unique = true)]
    public record DatabaseTrigger : BaseSqlView<Guid>
    {
        /// <summary> .cctor </summary>
        /// <param name="primaryKey">Primary key</param>
        /// <param name="schema">Schema</param>
        /// <param name="trigger">Trigger</param>
        /// <param name="table">Table</param>
        /// <param name="function">Function</param>
        /// <param name="type">Type</param>
        /// <param name="event">Event</param>
        public DatabaseTrigger(
            Guid primaryKey,
            string schema,
            string trigger,
            string table,
            string function,
            EnTriggerType type,
            EnTriggerEvent @event)
            : base(primaryKey)
        {
            Schema = schema;
            Trigger = trigger;
            Table = table;
            Function = function;
            Type = type;
            Event = @event;
        }

        /// <summary>
        /// Schema
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// Trigger
        /// </summary>
        public string Trigger { get; set; }

        /// <summary>
        /// Table
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        /// Function
        /// </summary>
        public string Function { get; set; }

        /// <summary>
        /// Type
        /// </summary>
        public EnTriggerType Type { get; set; }

        /// <summary>
        /// Event
        /// </summary>
        public EnTriggerEvent Event { get; set; }
    }
}