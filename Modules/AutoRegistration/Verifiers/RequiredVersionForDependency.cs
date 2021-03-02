namespace SpaceEngineers.Core.AutoRegistration.Verifiers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using AutoWiring.Api.Services;
    using Basics;
    using SimpleInjector;

    /// <summary>
    /// IVersionFor -> if version defined in code we must use it as dependency
    /// </summary>
    [Lifestyle(EnLifestyle.Singleton)]
    internal class RequiredVersionForDependency : IConfigurationVerifier
    {
        private readonly Container _container;
        private readonly IAutoWiringServicesProvider _servicesProvider;
        private readonly MethodInfo _isDecorator;

        public RequiredVersionForDependency(Container container, IAutoWiringServicesProvider servicesProvider)
        {
            _container = container;
            _servicesProvider = servicesProvider;

            _isDecorator = container
                          .GetType()
                          .Assembly
                          .GetType("SimpleInjector.Types")
                          .GetMethod("IsDecorator", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }

        public void Verify()
        {
            var servicesWithVersions = _servicesProvider.Versions().ToList();

            foreach (var instanceProducer in _container.GetCurrentRegistrations())
            {
                Flatten(instanceProducer)
                   .Where(pair => !IsDecorator(pair.ServiceType, pair.ImplementationType)
                               && !IsDecorator(pair.ServiceType, pair.Parent))
                   .Where(pair => servicesWithVersions.Contains(pair.ServiceType))
                   .Each(pair => throw new InvalidOperationException($"{pair.Parent.FullName} must depends on IVersioned<{pair.ServiceType.Name}> instead of {pair.ServiceType.Name}"));
            }
        }

        private IEnumerable<(Type ServiceType, Type ImplementationType, Type Parent)> Flatten(InstanceProducer producer)
        {
            return producer
                  .GetRelationships()
                  .Select(relationship => relationship.Dependency)
                  .SelectMany(child => new[] { Create(child, producer) }.Concat(Flatten(child)));

            (Type ServiceType, Type ImplementationType, Type Parent) Create(InstanceProducer child, InstanceProducer parent)
            {
                return (child.ServiceType, child.Registration.ImplementationType, parent.Registration.ImplementationType);
            }
        }

        private bool IsDecorator(Type serviceType, Type implementationType)
        {
            if (!implementationType.IsClass)
            {
                return false;
            }

            var constructorInfo = _container.Options.ConstructorResolutionBehavior.GetConstructor(implementationType);

            return (bool)_isDecorator.Invoke(null, new object[] { serviceType, constructorInfo });
        }
    }
}