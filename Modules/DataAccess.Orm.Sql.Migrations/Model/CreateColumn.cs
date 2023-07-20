namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Model
{
    /// <summary>
    /// CreateColumn
    /// </summary>
    public class CreateColumn : IModelChange
    {
        /// <summary> .cctor </summary>
        /// <param name="schema">Schema</param>
        /// <param name="table">Table</param>
        /// <param name="column">Column</param>
        public CreateColumn(
            string schema,
            string table,
            string column)
        {
            Schema = schema;
            Table = table;
            Column = column;
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
        /// Column
        /// </summary>
        public string Column { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(CreateColumn)} {Schema}.{Table}.{Column}";
        }
    }
}