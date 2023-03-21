namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    /// <summary>
    /// AlterEnumType
    /// </summary>
    public class AlterEnumType : IModelChange
    {
        /// <summary> .cctor </summary>
        /// <param name="schema">Schema</param>
        /// <param name="type">Type</param>
        /// <param name="oldValue">Old value</param>
        /// <param name="newValue">New value</param>
        public AlterEnumType(
            string schema,
            string type,
            string? oldValue,
            string? newValue)
        {
            Schema = schema;
            Type = type;
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Schema
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// Type
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Old value
        /// </summary>
        public string? OldValue { get; }

        /// <summary>
        /// New value
        /// </summary>
        public string? NewValue { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(AlterEnumType)} {Schema}.{Type} {OldValue ?? "(not existed)"} -> {NewValue ?? "(not existed)"}";
        }
    }
}