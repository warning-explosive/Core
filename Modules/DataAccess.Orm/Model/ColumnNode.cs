namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    using System;

    /// <summary>
    /// ColumnNode
    /// </summary>
    public class ColumnNode
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Column type</param>
        /// <param name="name">Column name</param>
        public ColumnNode(Type type, string name)
        {
            Type = type;
            Name = name;
        }

        /// <summary>
        /// Column type
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Column name
        /// </summary>
        public string Name { get; }
    }
}