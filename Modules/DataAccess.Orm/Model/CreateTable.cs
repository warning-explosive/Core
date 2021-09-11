namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    /// <summary>
    /// Create table change
    /// </summary>
    public class CreateTable : IDatabaseModelChange
    {
        /// <summary> .cctor </summary>
        /// <param name="table">Table</param>
        public CreateTable(TableNode table)
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
            return $"{nameof(CreateTable)} {Table.Type.Name}";
        }
    }
}