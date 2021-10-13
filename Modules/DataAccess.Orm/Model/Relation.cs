namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Relation
    /// </summary>
    public class Relation
    {
        /// <summary> .cctor </summary>
        /// <param name="property">Relation</param>
        /// <param name="type">Type</param>
        public Relation(PropertyInfo property, Type type)
        {
            Property = property;
            Type = type;
        }

        /// <summary>
        /// Property
        /// </summary>
        public PropertyInfo Property { get; }

        /// <summary>
        /// Type
        /// </summary>
        public Type Type { get; }
    }
}