namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    using System;

    /// <summary>
    /// Alter view change
    /// </summary>
    public class AlterView : IDatabaseModelChange
    {
        /// <summary> .cctor </summary>
        /// <param name="type">View type</param>
        /// <param name="query">View query</param>
        public AlterView(Type type, string query)
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
            return $"{nameof(AlterView)} {Type.Name}";
        }
    }
}