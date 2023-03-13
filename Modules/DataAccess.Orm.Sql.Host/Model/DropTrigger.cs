namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    /// <summary>
    /// DropTrigger
    /// </summary>
    public class DropTrigger : IModelChange
    {
        /// <summary> .cctor </summary>
        /// <param name="schema">Schema</param>
        /// <param name="trigger">Trigger</param>
        public DropTrigger(
            string schema,
            string trigger)
        {
            Schema = schema;
            Trigger = trigger;
        }

        /// <summary>
        /// Schema
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// Trigger
        /// </summary>
        public string Trigger { get; }
    }
}