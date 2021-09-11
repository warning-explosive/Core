namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    /// <summary>
    /// Add column change
    /// </summary>
    public class AddColumn : IDatabaseModelChange
    {
        /// <summary> .cctor </summary>
        /// <param name="table">Table</param>
        /// <param name="column">Column</param>
        public AddColumn(TableNode table, ColumnNode column)
        {
            Table = table;
            Column = column;
        }

        /// <summary>
        /// Table
        /// </summary>
        public TableNode Table { get; }

        /// <summary>
        /// Column
        /// </summary>
        public ColumnNode Column { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(AddColumn)} {Table.Type.Name}.{Column.Name}";
        }
    }
}