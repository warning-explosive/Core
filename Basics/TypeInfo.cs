namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using Attributes;

    [DebuggerDisplay("{OriginalType}")]
    internal class TypeInfo
    {
        internal TypeInfo(Type type)
        {
            OriginalType = type;

            Dependencies = type.GetCustomAttribute<DependencyAttribute>()?.Dependencies.ToArray()
                        ?? Array.Empty<Type>();

            ExtractDeclaredInterfaces(type);
            ExtractBaseTypes(type);
            ExtractOpenGenericInterfaces(type);
        }

        internal Type OriginalType { get; }

        internal Type[] Dependencies { get; }

        internal ICollection<Type> DeclaredInterfaces { get; } = new List<Type>();

        internal ICollection<Type> BaseTypes { get; } = new List<Type>();

        internal ICollection<Type> GenericTypeDefinitions { get; } = new List<Type>();

        internal ICollection<Type> GenericInterfaceDefinitions { get; } = new List<Type>();

        private void ExtractDeclaredInterfaces(Type type)
        {
            var allTypeInterfaces = type.GetInterfaces();
            var nestedInterfaces = allTypeInterfaces.SelectMany(i => i.GetInterfaces());

            var declaredInterfaces = allTypeInterfaces.Except(nestedInterfaces);

            var currentType = type;
            while (currentType.BaseType != null)
            {
                declaredInterfaces = declaredInterfaces.Except(currentType.BaseType.GetInterfaces());

                currentType = currentType.BaseType;
            }

            declaredInterfaces.Each(i => DeclaredInterfaces.Add(i));
        }

        private void ExtractBaseTypes(Type currentType)
        {
            while (currentType.BaseType != null)
            {
                BaseTypes.Add(currentType.BaseType);

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
    }
}