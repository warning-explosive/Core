namespace SpaceEngineers.Core.AutoRegistration.Verifiers
{
    using System;
    using System.Linq;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using AutoWiring.Api.Services;
    using Basics;
    using Extensions;
    using Internals;
    using SimpleInjector;

    /// <summary>
    /// IVersionFor -> if version defined in code we must use it as dependency
    /// </summary>
    [Lifestyle(EnLifestyle.Singleton)]
    internal class RequiredVersionForDependency : IConfigurationVerifier
    {
        private readonly Container _container;
        private readonly IAutoWiringServicesProvider _servicesProvider;

        public RequiredVersionForDependency(Container container, IAutoWiringServicesProvider servicesProvider)
        {
            _container = container;
            _servicesProvider = servicesProvider;
        }

        public void Verify()
        {
            var servicesWithVersions = _servicesProvider.Versions().ToList();

            foreach (var instanceProducer in _container.GetCurrentRegistrations())
            {
                DependencyInfo
                    .RetrieveDependencyGraph(instanceProducer)
                    .ExtractFromGraph(dependency => dependency)
                    .Where(pair => !IsDecorator(pair.ServiceType, pair.ImplementationType)
                                   && pair.Parent != null
                                   && !IsDecorator(pair.ServiceType, pair.Parent.ImplementationType))
                    .Where(pair => servicesWithVersions.Contains(pair.ServiceType))
                    .Each(pair => throw new InvalidOperationException($"{pair.Parent.ImplementationType.FullName} must depends on IVersioned<{pair.ServiceType.Name}> instead of {pair.ServiceType.Name}"));
            }
        }

        private bool IsDecorator(Type serviceType, Type implementationType)
        {
            if (!implementationType.IsClass)
            {
                return false;
            }

            return _container
                .Options
                .ConstructorResolutionBehavior
                .GetConstructor(implementationType)
                .IsDecorator(serviceType);
        }
    }
}