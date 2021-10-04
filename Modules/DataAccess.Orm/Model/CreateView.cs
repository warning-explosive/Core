namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    /// <summary>
    /// CreateView
    /// </summary>
    public class CreateView : IDatabaseModelChange
    {
        /// <summary> .cctor </summary>
        /// <param name="schema">Schema</param>
        /// <param name="view">View</param>
        /// <param name="query">Query</param>
        public CreateView(
            string schema,
            string view,
            string query)
        {
            Schema = schema;
            View = view;
            Query = query;
        }

        /// <summary>
        /// Schema
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// View
        /// </summary>
        public string View { get; }

        /// <summary>
        /// Query
        /// </summary>
        public string Query { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(CreateView)} {Schema}.{View}";
        }
    }
}