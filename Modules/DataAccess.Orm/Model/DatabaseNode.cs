namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    using System.Collections.Generic;

    /// <summary>
    /// DatabaseNode
    /// </summary>
    public class DatabaseNode
    {
        /// <summary> .cctor </summary>
        /// <param name="name">Database name</param>
        /// <param name="schemas">Database schemas</param>
        public DatabaseNode(string name, IReadOnlyCollection<SchemaNode> schemas)
        {
            Name = name;
            Schemas = schemas;
        }

        /// <summary>
        /// Database name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Database schemas
        /// </summary>
        public IReadOnlyCollection<SchemaNode> Schemas { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }
    }
}