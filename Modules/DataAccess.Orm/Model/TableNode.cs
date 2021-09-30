namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// TableNode
    /// </summary>
    public class TableNode
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Table type</param>
        /// <param name="columns">Columns</param>
        public TableNode(Type type, IReadOnlyCollection<ColumnNode> columns)
        {
            Type = type;
            Name = type.Name;
            Columns = columns;
        }

        /// <summary> .cctor </summary>
        /// <param name="name">Table name</param>
        /// <param name="columns">Columns</param>
        public TableNode(string name, IReadOnlyCollection<ColumnNode> columns)
        {
            Name = name;
            Columns = columns;
        }

        /// <summary>
        /// Table type
        /// </summary>
        public Type? Type { get; }

        /// <summary>
        /// Table name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Columns
        /// </summary>
        public IReadOnlyCollection<ColumnNode> Columns { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }
    }
}