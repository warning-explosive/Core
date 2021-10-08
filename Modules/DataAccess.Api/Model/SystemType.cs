namespace SpaceEngineers.Core.DataAccess.Api.Model
{
    using System;
    using Basics;

    /// <summary>
    /// System type
    /// </summary>
    public record SystemType : IInlinedObject
    {
        /// <summary> .cctor </summary>
        /// <param name="assembly">Assembly</param>
        /// <param name="type">Type</param>
        public SystemType(string assembly, string type)
        {
            Assembly = assembly;
            Type = type;
        }

        /// <summary>
        /// Assembly
        /// </summary>
        public string Assembly { get; private init; }

        /// <summary>
        /// Type
        /// </summary>
        public string Type { get; private init; }

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
            return AssembliesExtensions.FindRequiredType(Assembly, Type);
        }

        /// <summary>
        /// Creates SystemType from System.Type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>SystemType</returns>
        public static SystemType FromType(Type type)
        {
            return new SystemType(type.Assembly.GetName().Name, type.FullName);
        }
    }
}