namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;

    /// <summary>
    /// Relation
    /// </summary>
    public class Relation
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Name</param>
        public Relation(
            Type type,
            string name)
        {
            Type = type;
            Name = name;
        }

        /// <summary>
        /// Type
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Property
        /// </summary>
        public string Name { get; }
    }
}