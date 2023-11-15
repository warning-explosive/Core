namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Model
{
    /// <summary>
    /// CreateTrigger
    /// </summary>
    public class CreateTrigger : IModelChange
    {
        /// <summary> .cctor </summary>
        /// <param name="trigger">Trigger</param>
        /// <param name="schema">Schema</param>
        /// <param name="table">Table</param>
        /// <param name="function">Function</param>
        /// <param name="type">Type</param>
        /// <param name="event">Event</param>
        public CreateTrigger(
            string trigger,
            string schema,
            string table,
            string function,
            EnTriggerType type,
            EnTriggerEvent @event)
        {
            Trigger = trigger;
            Schema = schema;
            Table = table;
            Function = function;
            Type = type;
            Event = @event;
        }

        /// <summary>
        /// Trigger
        /// </summary>
        public string Trigger { get; }

        /// <summary>
        /// Schema
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// Table
        /// </summary>
        public string Table { get; }

        /// <summary>
        /// Function
        /// </summary>
        public string Function { get; }

        /// <summary>
        /// Type
        /// </summary>
        public EnTriggerType Type { get; }

        /// <summary>
        /// Event
        /// </summary>
        public EnTriggerEvent Event { get; }
    }
}