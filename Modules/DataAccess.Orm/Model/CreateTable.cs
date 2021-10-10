namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    /// <summary>
    /// CreateTable
    /// </summary>
    public class CreateTable : IModelChange
    {
        /// <summary> .cctor </summary>
        /// <param name="schema">Schema</param>
        /// <param name="table">Table</param>
        public CreateTable(string schema, string table)
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
            return $"{nameof(CreateTable)} {Schema}.{Table}";
        }
    }
}