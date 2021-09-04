namespace SpaceEngineers.Core.DataAccess.Orm.Model.Abstractions
{
    using System;

    /// <summary>
    /// Upsert view change
    /// </summary>
    public class UpsertView : IDatabaseModelChange
    {
        /// <summary> .cctor </summary>
        /// <param name="type">View type</param>
        /// <param name="query">View query</param>
        public UpsertView(Type type, string query)
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
            return $"{nameof(UpsertView)} {Type.Name}";
        }
    }
}