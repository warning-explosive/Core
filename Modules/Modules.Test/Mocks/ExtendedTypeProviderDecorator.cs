namespace SpaceEngineers.Core.Modules.Test.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Component(EnLifestyle.Singleton, EnComponentRegistrationKind.Override)]
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

        internal static Func<DependencyContainerOptions, DependencyContainerOptions> ExtendTypeProvider(params Type[] additionalTypes)
        {
            return options => ExtendTypeProvider(options, additionalTypes);
        }

        internal static DependencyContainerOptions ExtendTypeProvider(DependencyContainerOptions options, params Type[] additionalTypes)
        {
            return options.WithManualRegistration(new ExtendedTypeProviderManualRegistration(additionalTypes));
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
                    .RegisterDecorator<ITypeProvider, ExtendedTypeProviderDecorator>()
                    .RegisterInstance(new TypeProviderExtension(_additionalTypes));
            }
        }
    }
}