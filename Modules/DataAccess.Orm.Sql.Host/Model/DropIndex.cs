namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    /// <summary>
    /// DropIndex
    /// </summary>
    public class DropIndex : IModelChange
    {
        /// <summary> .cctor </summary>
        /// <param name="schema">Schema</param>
        /// <param name="table">Table</param>
        /// <param name="index">Index</param>
        public DropIndex(
            string schema,
            string table,
            string index)
        {
            Schema = schema;
            Table = table;
            Index = index;
        }

        /// <summary>
        /// Schema
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// Table
        /// </summary>
        public string Table { get; }

        /// <summary>
        /// Index
        /// </summary>
        public string Index { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(DropIndex)} {Schema}.{Table}.{Index}";
        }
    }
}