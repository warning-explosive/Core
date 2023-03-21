namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;

    [DebuggerDisplay("{OriginalType}")]
    internal class TypeInfo
    {
        private readonly Lazy<IReadOnlyCollection<Type>> _dependencies;
        private readonly Lazy<IReadOnlyCollection<Type>> _baseTypes;
        private readonly Lazy<IReadOnlyCollection<Type>> _genericTypeDefinitions;
        private readonly Lazy<IReadOnlyCollection<Type>> _declaredInterfaces;
        private readonly Lazy<IReadOnlyCollection<Type>> _genericInterfaceDefinitions;
        private readonly Lazy<IReadOnlyCollection<Attribute>> _attributes;

        internal TypeInfo(Type type)
        {
            OriginalType = type;

            _dependencies = new Lazy<IReadOnlyCollection<Type>>(() => TypeInfoStorage.ExtractDependencies(type).ToList(), LazyThreadSafetyMode.ExecutionAndPublication);
            _baseTypes = new Lazy<IReadOnlyCollection<Type>>(() => ExtractBaseTypes(type).ToList(), LazyThreadSafetyMode.ExecutionAndPublication);
            _genericTypeDefinitions = new Lazy<IReadOnlyCollection<Type>>(() => ExtractGenericTypeDefinitions(type).ToList(), LazyThreadSafetyMode.ExecutionAndPublication);
            _declaredInterfaces = new Lazy<IReadOnlyCollection<Type>>(() => ExtractDeclaredInterfaces(type).ToList(), LazyThreadSafetyMode.ExecutionAndPublication);
            _genericInterfaceDefinitions = new Lazy<IReadOnlyCollection<Type>>(() => ExtractGenericInterfaceDefinitions(type).ToList(), LazyThreadSafetyMode.ExecutionAndPublication);
            _attributes = new Lazy<IReadOnlyCollection<Attribute>>(() => ExtractAttributes(type).ToList(), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public Type OriginalType { get; }

        public IReadOnlyCollection<Type> Dependencies => _dependencies.Value;

        public IReadOnlyCollection<Type> BaseTypes => _baseTypes.Value;

        public IReadOnlyCollection<Type> GenericTypeDefinitions => _genericTypeDefinitions.Value;

        public IReadOnlyCollection<Type> DeclaredInterfaces => _declaredInterfaces.Value;

        public IReadOnlyCollection<Type> GenericInterfaceDefinitions => _genericInterfaceDefinitions.Value;

        public IReadOnlyCollection<Attribute> Attributes => _attributes.Value;

        private static IEnumerable<Type> ExtractBaseTypes(Type type)
        {
            var currentType = type;

            while (currentType.BaseType != null)
            {
                yield return currentType.BaseType;

                currentType = currentType.BaseType;
            }
        }

        private static IEnumerable<Type> ExtractGenericTypeDefinitions(Type type)
        {
            var currentType = type;

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

        private static IEnumerable<Attribute> ExtractAttributes(Type type)
        {
            return type.GetCustomAttributes(true)
                       .OfType<Attribute>();
        }
    }
}