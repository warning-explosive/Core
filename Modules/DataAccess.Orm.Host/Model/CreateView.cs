namespace SpaceEngineers.Core.DataAccess.Orm.Host.Model
{
    /// <summary>
    /// CreateView
    /// </summary>
    public class CreateView : IModelChange
    {
        /// <summary> .cctor </summary>
        /// <param name="schema">Schema</param>
        /// <param name="view">View</param>
        public CreateView(string schema, string view)
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
            return $"{nameof(CreateView)} {Schema}.{View}";
        }
    }
}