namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    /// <summary>
    /// DropTable
    /// </summary>
    public class DropTable : IModelChange
    {
        /// <summary> .cctor </summary>
        /// <param name="schema">Schema</param>
        /// <param name="table">Table</param>
        public DropTable(string schema, string table)
        {
            Schema = schema;
            Table = table;
        }

        /// <summary>
        /// Schema
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// Table
        /// </summary>
        public string Table { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(DropTable)} {Schema}.{Table}";
        }
    }
}