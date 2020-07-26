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

        private readonly ITypeExtensions _typeExtensions;

        /// <summary> .cctor </summary>
        /// <param name="container">Container</param>
        /// <param name="typeExtensions">ITypeExtensions</param>
        public AutoWiringServicesProvider(Container container, ITypeExtensions typeExtensions)
        {
            _container = container;
            _typeExtensions = typeExtensions;
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
            return typeof(IResolvableImplementation).GetTypesToRegister(_container, _typeExtensions);
        }

        /// <inheritdoc />
        public IEnumerable<Type> External()
        {
            return _typeExtensions
                  .AllLoadedTypes()
                  .Where(type => type.IsClass
                              && !type.IsAbstract
                              && _typeExtensions.IsSubclassOfOpenGeneric(type, typeof(IExternalResolvable<>)))
                  .SelectMany(type => _typeExtensions.GetGenericArgumentsOfOpenGenericAt(type, typeof(IExternalResolvable<>), 0))
                  .Select(_typeExtensions.ExtractGenericTypeDefinition)
                  .Distinct();
        }

        /// <inheritdoc />
        public IEnumerable<Type> Versions()
        {
            return _typeExtensions
                  .OurTypes()
                  .Where(z => z.IsClass
                           && !z.IsAbstract
                           && _typeExtensions.IsSubclassOfOpenGeneric(z, typeof(IVersionFor<>)))
                  .SelectMany(z => z.GetInterfaces().Where(i => i.IsGenericType && !i.IsGenericTypeDefinition && i.GetGenericTypeDefinition() == typeof(IVersionFor<>)))
                  .Distinct()
                  .Select(version => version.GenericTypeArguments[0]);
        }

        private Type[] Services(Type serviceType)
        {
            if (!serviceType.IsInterface
             || serviceType.IsGenericType)
            {
                throw new ArgumentException(serviceType.FullName);
            }

            return _typeExtensions
                  .OurTypes()
                  .Where(t => t.IsInterface
                           && _typeExtensions.IsContainsInterfaceDeclaration(t, serviceType))
                  .ToArray();
        }
    }
}