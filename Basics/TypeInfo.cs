namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using Attributes;

    [DebuggerDisplay("{OriginalType}")]
    internal class TypeInfo : ITypeInfo
    {
        internal TypeInfo(Type type)
        {
            OriginalType = type;
            Dependencies = type.GetCustomAttribute<DependencyAttribute>()?.Dependencies ?? new List<Type>();
            BaseTypes = ExtractBaseTypes(type).ToList();
            GenericTypeDefinitions = ExtractGenericTypeDefinitions(type).ToList();
            DeclaredInterfaces = ExtractDeclaredInterfaces(type).ToList();
            GenericInterfaceDefinitions = ExtractGenericInterfaceDefinitions(type).ToList();
            Attributes = ExtractAttributes(type).ToList();
        }

        public Type OriginalType { get; }

        public IReadOnlyCollection<Type> Dependencies { get; }

        public IReadOnlyCollection<Type> BaseTypes { get; }

        public IReadOnlyCollection<Type> GenericTypeDefinitions { get; }

        public IReadOnlyCollection<Type> DeclaredInterfaces { get; }

        public IReadOnlyCollection<Type> GenericInterfaceDefinitions { get; }

        public IReadOnlyCollection<Attribute> Attributes { get; }

        private static IEnumerable<Type> ExtractBaseTypes(Type currentType)
        {
            while (currentType.BaseType != null)
            {
                yield return currentType.BaseType;

                currentType = currentType.BaseType;
            }
        }

        private static IEnumerable<Type> ExtractGenericTypeDefinitions(Type currentType)
        {
            while (currentType != null)
            {
                if (currentType.IsGenericType)
                {
                    yield return currentType.GetGenericTypeDefinition();
                }

                currentType = currentType.BaseType;
            }
        }

        private static IEnumerable<Type> ExtractDeclaredInterfaces(Type type)
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

            foreach (var i in declaredInterfaces)
            {
                yield return i;
            }
        }

        private static IEnumerable<Type> ExtractGenericInterfaceDefinitions(Type type)
        {
            var source = type.GetInterfaces().AsEnumerable();

            if (type.IsInterface)
            {
                source = new[] { type }.Concat(source);
            }

            foreach (var i in source.Where(i => i.IsGenericType))
            {
                yield return i.GetGenericTypeDefinition();
            }
        }

        private IEnumerable<Attribute> ExtractAttributes(Type type)
        {
            return type.GetCustomAttributes(true)
                       .OfType<Attribute>();
        }
    }
}