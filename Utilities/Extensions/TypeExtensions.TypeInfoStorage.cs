namespace SpaceEngineers.Core.Utilities.Extensions
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;

    internal static class TypeInfoStorage
    {
        public static ConcurrentDictionary<Type, TypeInfo> Collection { get; } = new ConcurrentDictionary<Type, TypeInfo>();

        internal class TypeInfo
        {
            public ICollection<Type> GenericTypeDefinitions { get; } = new List<Type>();

            public ICollection<Type> GenericInterfaceDefinitions { get; } = new List<Type>();
            
            public ICollection<Type> DeclaredInterfaces { get; } = new List<Type>();

            public TypeInfo([NotNull] Type type)
            {
                ExtractBaseOpenGenericTypes(type);
                ExtractOpenGenericInterfaces(type);
                ExtractDeclaredInterfaces(type);
            }
            
            private void ExtractBaseOpenGenericTypes(Type currentType)
            {
                while (currentType.BaseType != null)
                {
                    if (currentType.IsGenericType)
                    {
                        GenericTypeDefinitions.Add(currentType.GetGenericTypeDefinition());
                    }

                    currentType = currentType.BaseType;
                }
            }

            private void ExtractOpenGenericInterfaces(Type type)
            {
                foreach (var i in type.GetInterfaces().Where(i => i.IsGenericType))
                {
                    GenericInterfaceDefinitions.Add(i.GetGenericTypeDefinition());
                }
            }

            private void ExtractDeclaredInterfaces(Type type)
            {
                var allTypeInterfaces = type.GetInterfaces();
                var nestedInterfaces = allTypeInterfaces.SelectMany(i => i.GetInterfaces());

                var declaredInterfaces = allTypeInterfaces.Except(nestedInterfaces);
                
                if (type.BaseType != null)
                {
                    declaredInterfaces = declaredInterfaces.Except(type.BaseType.GetInterfaces());
                }
                
                foreach (var i in declaredInterfaces)
                {
                    DeclaredInterfaces.Add(i);
                }
            }
        }
    }
}