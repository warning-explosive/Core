namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    /// <summary>
    /// DropEnumType
    /// </summary>
    public class DropEnumType : IModelChange
    {
        /// <summary> .cctor </summary>
        /// <param name="schema">Schema</param>
        /// <param name="type">Type</param>
        public DropEnumType(
            string schema,
            string type)
        {
            Schema = schema;
            Type = type;
        }

        /// <summary>
        /// Schema
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// Type
        /// </summary>
        public string Type { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(DropEnumType)} {Schema}.{Type}";
        }
    }
}