namespace SpaceEngineers.Core.DataAccess.Orm.Model.Abstractions
{
    /// <summary>
    /// Drop table change
    /// </summary>
    public class DropTable : IDatabaseModelChange
    {
        /// <summary> .cctor </summary>
        /// <param name="table">Table</param>
        public DropTable(TableNode table)
        {
            Table = table;
        }

        /// <summary>
        /// Table
        /// </summary>
        public TableNode Table { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(DropTable)} {Table.Name}";
        }
    }
}