namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    using System;

    /// <summary>
    /// ViewNode
    /// </summary>
    public class ViewNode
    {
        /// <summary> .cctor </summary>
        /// <param name="type">View type</param>
        /// <param name="query">View query</param>
        public ViewNode(Type type, string query)
        {
            Type = type;
            Name = type.Name;
            Query = query;
        }

        /// <summary> .cctor </summary>
        /// <param name="name">View name</param>
        /// <param name="query">View query</param>
        public ViewNode(string name, string query)
        {
            Name = name;
            Query = query;
        }

        /// <summary>
        /// View type
        /// </summary>
        public Type? Type { get; }

        /// <summary>
        /// View name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// View query
        /// </summary>
        public string Query { get; }
    }
}