namespace SpaceEngineers.Core.Modules.Test.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using AutoWiring.Api.Services;

    [Component(EnLifestyle.Singleton, EnComponentKind.Override)]
    internal class TypeProviderMock : ITypeProvider, IDecorator<ITypeProvider>
    {
        private readonly ITypeProvider _decoratee;
        private readonly IReadOnlyCollection<Type> _additionalOurTypes;

        public TypeProviderMock(ITypeProvider decoratee, IReadOnlyCollection<Type> additionalOurOurTypes)
        {
            _decoratee = decoratee;
            _additionalOurTypes = additionalOurOurTypes;
        }

        public ITypeProvider Decoratee => _decoratee;

        public IReadOnlyCollection<Assembly> AllLoadedAssemblies => _decoratee.AllLoadedAssemblies;

        public IReadOnlyCollection<Type> AllLoadedTypes => _decoratee.AllLoadedTypes.Concat(_additionalOurTypes).ToList();

        public IReadOnlyCollection<Assembly> OurAssemblies => _decoratee.OurAssemblies;

        public IReadOnlyCollection<Type> OurTypes => _decoratee.OurTypes.Concat(_additionalOurTypes).ToList();

        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, Type>> TypeCache => _decoratee.TypeCache;

        public bool IsOurType(Type type) => _decoratee.IsOurType(type);
    }
}