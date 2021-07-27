namespace SpaceEngineers.Core.DataAccess.Orm.Model.Abstractions
{
    /// <summary>
    /// Drop column change
    /// </summary>
    public class DropColumn : IDatabaseModelChange
    {
        /// <summary> .cctor </summary>
        /// <param name="table">Table</param>
        /// <param name="column">Column</param>
        public DropColumn(TableNode table, ColumnNode column)
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
            return $"{nameof(DropColumn)} {Table.Type.Name}.{Column.Name}";
        }
    }
}