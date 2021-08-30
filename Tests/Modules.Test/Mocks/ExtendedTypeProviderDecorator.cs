namespace SpaceEngineers.Core.Modules.Test.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using CompositionRoot.Api.Abstractions;

    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    [ComponentOverride]
    internal class ExtendedTypeProviderDecorator : ITypeProvider, IDecorator<ITypeProvider>
    {
        private readonly ITypeProvider _decoratee;
        private readonly TypeProviderExtension _extension;

        public ExtendedTypeProviderDecorator(ITypeProvider decoratee, TypeProviderExtension extension)
        {
            _decoratee = decoratee;
            _extension = extension;
        }

        #region ITypeProvider

        public ITypeProvider Decoratee => _decoratee;

        public IReadOnlyCollection<Assembly> AllLoadedAssemblies => _decoratee.AllLoadedAssemblies;

        public IReadOnlyCollection<Type> AllLoadedTypes => _decoratee.AllLoadedTypes.Concat(_extension.OurTypes).Distinct().ToList();

        public IReadOnlyCollection<Assembly> OurAssemblies => _decoratee.OurAssemblies;

        public IReadOnlyCollection<Type> OurTypes => _decoratee.OurTypes.Concat(_extension.OurTypes).Distinct().ToList();

        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, Type>> TypeCache => _decoratee.TypeCache;

        public bool IsOurType(Type type)
        {
            return _decoratee.IsOurType(type)
                   || _extension.OurTypes.Contains(type);
        }

        #endregion

        internal class TypeProviderExtension
        {
            public TypeProviderExtension(IReadOnlyCollection<Type> ourTypes)
            {
                OurTypes = ourTypes;
            }

            public IReadOnlyCollection<Type> OurTypes { get; }
        }
    }
}