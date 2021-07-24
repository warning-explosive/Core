namespace SpaceEngineers.Core.DataAccess.Orm.Model.Abstractions
{
    using System.Collections.Generic;

    /// <summary>
    /// DatabaseNode
    /// </summary>
    public class DatabaseNode
    {
        /// <summary> .cctor </summary>
        /// <param name="name">Database name</param>
        /// <param name="tables">Database tables</param>
        public DatabaseNode(string name, IReadOnlyCollection<TableNode> tables)
        {
            Tables = tables;
            Name = name;
        }

        /// <summary>
        /// Database name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Tables
        /// </summary>
        public IReadOnlyCollection<TableNode> Tables { get; }
    }
}