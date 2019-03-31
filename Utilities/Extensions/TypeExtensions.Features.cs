namespace SpaceEngineers.Core.Utilities.Extensions
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// System.Type extensions methods
    /// </summary>
    public static partial class TypeExtensions
    {
        private static readonly ConcurrentDictionary<Type, TypeInfo> TypeCollection = new ConcurrentDictionary<Type, TypeInfo>();
        
        /// <summary>
        /// Does type implement open-generic base type
        /// </summary>
        /// <param name="type">Type for check</param>
        /// <param name="openGenericBaseType">Open generic base type</param>
        /// <returns>Result of check</returns>
        public static bool IsImplementationOfOpenGeneric(this Type type, Type openGenericBaseType)
        {
            var types = TypeCollection.GetOrAdd(type, t => new TypeInfo(t)).GenericTypeDefinitionOfBaseTypes;

            return openGenericBaseType.IsGenericType
                   && openGenericBaseType.IsGenericTypeDefinition
                   && types.Contains(openGenericBaseType);
        }
        
        /// <summary>
        /// Does type implement open-generic interface
        /// </summary>
        /// <param name="type">Type for check</param>
        /// <param name="openGenericInterface">Open generic interface</param>
        /// <returns>Result of check</returns>
        public static bool IsImplementationOfOpenGenericInterface(this Type type, Type openGenericInterface)
        {
            var types = TypeCollection.GetOrAdd(type, t => new TypeInfo(t)).GenericTypeDefinitionOfInterfaces;

            return openGenericInterface.IsInterface
                   && openGenericInterface.IsGenericType
                   && openGenericInterface.IsGenericTypeDefinition
                   && types.Contains(openGenericInterface);
        }
        
        private class TypeInfo
        {
            public TypeInfo(Type type)
            {
                var currentType = type;
                
                while (currentType != null)
                {
                    if (currentType.IsGenericType)
                    {
                        GenericTypeDefinitionOfBaseTypes.Add(currentType.GetGenericTypeDefinition());
                    }

                    currentType = currentType.BaseType;
                }

                GenericTypeDefinitionOfInterfaces.AddRange(type.GetInterfaces()
                                                               .Where(i => i.IsGenericType)
                                                               .Select(i => i.GetGenericTypeDefinition()));
            }
            
            public List<Type> GenericTypeDefinitionOfBaseTypes { get; } = new List<Type>();
            
            public List<Type> GenericTypeDefinitionOfInterfaces { get; } = new List<Type>();
        }
    }
}