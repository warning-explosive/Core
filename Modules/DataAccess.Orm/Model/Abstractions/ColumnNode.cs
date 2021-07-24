namespace SpaceEngineers.Core.DataAccess.Orm.Model.Abstractions
{
    using System;

    /// <summary>
    /// ColumnNode
    /// </summary>
    public class ColumnNode : IModelNode
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Column type</param>
        /// <param name="name">Column name</param>
        public ColumnNode(Type type, string name)
        {
            Type = type;
            Name = name;
        }

        /// <inheritdoc />
        public Type Type { get; }

        /// <inheritdoc />
        public string Name { get; }
    }
}