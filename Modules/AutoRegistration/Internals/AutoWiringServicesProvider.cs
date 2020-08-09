namespace SpaceEngineers.Core.AutoRegistration.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Basics;
    using SimpleInjector;

    /// <inheritdoc />
    [Lifestyle(EnLifestyle.Singleton)]
    [Unregistered]
    internal class AutoWiringServicesProvider : IAutoWiringServicesProvider
    {
        private readonly Container _container;

        private readonly ITypeProvider _typeProvider;

        /// <summary> .cctor </summary>
        /// <param name="container">Container</param>
        /// <param name="typeProvider">ITypeProvider</param>
        public AutoWiringServicesProvider(Container container, ITypeProvider typeProvider)
        {
            _container = container;
            _typeProvider = typeProvider;
        }

        /// <inheritdoc />
        public IEnumerable<Type> Resolvable()
        {
            return Services(typeof(IResolvable));
        }

        /// <inheritdoc />
        public IEnumerable<Type> Collections()
        {
            return Services(typeof(ICollectionResolvable));
        }

        /// <inheritdoc />
        public IEnumerable<Type> Implementations()
        {
            return typeof(IResolvableImplementation).GetTypesToRegister(_container, _typeProvider);
        }

        /// <inheritdoc />
        public IEnumerable<Type> External()
        {
            return _typeProvider
                  .AllLoadedTypes
                  .Where(type => type.IsClass
                              && !type.IsAbstract
                              && type.IsSubclassOfOpenGeneric(typeof(IExternalResolvable<>)))
                  .SelectMany(type => type.ExtractGenericArgumentsAt(typeof(IExternalResolvable<>), 0))
                  .Where(type => !type.IsGenericParameter)
                  .Select(type => type.GenericTypeDefinitionOrSelf())
                  .Distinct();
        }

        /// <inheritdoc />
        public IEnumerable<Type> Versions()
        {
            return _typeProvider
                  .OurTypes
                  .Where(type => type.IsClass
                           && !type.IsAbstract
                           && type.IsSubclassOfOpenGeneric(typeof(IVersionFor<>)))
                  .SelectMany(type => type.ExtractGenericArgumentsAt(typeof(IVersionFor<>), 0))
                  .Where(type => !type.IsGenericParameter)
                  .Distinct();
        }

        private Type[] Services(Type serviceType)
        {
            if (!serviceType.IsInterface
             || serviceType.IsGenericType)
            {
                throw new ArgumentException(serviceType.FullName);
            }

            return _typeProvider
                  .OurTypes
                  .Where(t => t.IsInterface && t.IsContainsInterfaceDeclaration(serviceType)) // TODO: review this approach (use attributes with inheritance=false OR register all derived interfaces)
                  .ToArray();
        }
    }
}