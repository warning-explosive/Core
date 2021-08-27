namespace SpaceEngineers.Core.Modules.Test.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions;

    // TODO: [ComponentOverride]
    [UnregisteredComponent]
    internal class ExtendedTypeProviderDecorator : ITypeProvider, IDecorator<ITypeProvider>
    {
        private readonly ITypeProvider _decoratee;
        private readonly TypeProviderExtension _extension;

        public ExtendedTypeProviderDecorator(ITypeProvider decoratee, TypeProviderExtension extension)
        {
            _decoratee = decoratee;
            _extension = extension;
        }

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

        internal static Func<DependencyContainerOptions, DependencyContainerOptions> ExtendTypeProvider(params Type[] additionalOurTypes)
        {
            return options => ExtendTypeProvider(options, additionalOurTypes);
        }

        internal static DependencyContainerOptions ExtendTypeProvider(DependencyContainerOptions options, params Type[] additionalOurTypes)
        {
            return options.WithManualRegistrations(new ExtendedTypeProviderManualRegistration(additionalOurTypes));
        }

        [Component(EnLifestyle.Singleton)]
        internal class TypeProviderExtension
        {
            public TypeProviderExtension(IReadOnlyCollection<Type> ourTypes)
            {
                OurTypes = ourTypes;
            }

            public IReadOnlyCollection<Type> OurTypes { get; }
        }

        private class ExtendedTypeProviderManualRegistration : IManualRegistration
        {
            private readonly IReadOnlyCollection<Type> _additionalTypes;

            public ExtendedTypeProviderManualRegistration(IReadOnlyCollection<Type> additionalTypes)
            {
                _additionalTypes = additionalTypes;
            }

            public void Register(IManualRegistrationsContainer container)
            {
                container
                    .RegisterDecorator<ITypeProvider, ExtendedTypeProviderDecorator>(EnLifestyle.Singleton)
                    .RegisterInstance(new TypeProviderExtension(_additionalTypes));
            }
        }
    }
}