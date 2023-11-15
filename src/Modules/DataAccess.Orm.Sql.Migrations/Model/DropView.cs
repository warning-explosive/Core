namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Model
{
    /// <summary>
    /// DropView
    /// </summary>
    public class DropView : IModelChange
    {
        /// <summary> .cctor </summary>
        /// <param name="schema">Schema</param>
        /// <param name="view">View</param>
        public DropView(string schema, string view)
        {
            Schema = schema;
            View = view;
        }

        /// <summary>
        /// Schema
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// View
        /// </summary>
        public string View { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(DropView)} {Schema}.{View}";
        }
    }
}