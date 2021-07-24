namespace SpaceEngineers.Core.DataAccess.Orm.Model.Abstractions
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// TableNode
    /// </summary>
    public class TableNode : IModelNode
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Table type</param>
        /// <param name="name">Table name</param>
        /// <param name="columns">Columns</param>
        public TableNode(Type type, string name, IReadOnlyCollection<ColumnNode> columns)
        {
            Type = type;
            Name = name;
            Columns = columns;
        }

        /// <inheritdoc />
        public Type Type { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <summary>
        /// Columns
        /// </summary>
        public IReadOnlyCollection<ColumnNode> Columns { get; }
    }
}