namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    using System;

    /// <summary>
    /// Create view change
    /// </summary>
    public class CreateView : IDatabaseModelChange
    {
        /// <summary> .cctor </summary>
        /// <param name="type">View type</param>
        /// <param name="query">View query</param>
        public CreateView(Type type, string query)
        {
            Type = type;
            Query = query;
        }

        /// <summary>
        /// View type
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// View query
        /// </summary>
        public string Query { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(CreateView)} {Type.Name}";
        }
    }
}