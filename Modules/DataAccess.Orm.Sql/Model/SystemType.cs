namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using Basics;

    /// <summary>
    /// System type
    /// </summary>
    public record SystemType : IInlinedObject
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        public SystemType(string type)
        {
            Type = type;
        }

        /// <summary>
        /// Type
        /// </summary>
        public string Type { get; init; }

        /// <summary>
        /// Implicit conversion operator to System.Type
        /// </summary>
        /// <param name="systemType">SystemType</param>
        /// <returns>Type</returns>
        public static implicit operator Type(SystemType systemType) => systemType.ToType();

        /// <summary>
        /// Implicit conversion operator from System.Type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>SystemType</returns>
        public static implicit operator SystemType(Type type) => FromType(type);

        /// <summary>
        /// Converts to System.Type
        /// </summary>
        /// <returns>Type</returns>
        public Type ToType()
        {
            return TypeExtensions.FindType(Type);
        }

        /// <summary>
        /// Creates SystemType from System.Type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>SystemType</returns>
        public static SystemType FromType(Type type)
        {
            return new SystemType(TypeNode.FromType(type).ToString());
        }
    }
}