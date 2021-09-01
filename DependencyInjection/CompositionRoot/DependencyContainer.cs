namespace SpaceEngineers.Core.CompositionRoot
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using Abstractions;
    using Api.Abstractions;
    using Api.Exceptions;
    using AutoRegistration.Api.Attributes;
    using Basics;
    using Extensions;
    using Implementations;
    using ManualRegistrations;
    using Registration;

    /// <summary>
    /// Dependency container implementation based on SimpleInjector
    /// Resolve dependencies by 'Dependency Injection' patterns
    /// Don't use it like ServiceLocator!
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    [ManuallyRegisteredComponent("Is created manually and implicitly during DependencyContainer initialization")]
    public class DependencyContainer : IDependencyContainer, IDisposable
    {
        private DependencyContainerOptions _options;

        /// <summary> .cctor </summary>
        /// <param name="implementation">IDependencyContainerImplementation</param>
        /// <param name="typeProvider">ContainerDependentTypeProvider</param>
        /// <param name="options">DependencyContainerOptions</param>
        [Obsolete("Use factory methods instead")]
        public DependencyContainer(
            IDependencyContainerImplementation implementation,
            DependencyContainerOptions options,
            ITypeProvider typeProvider)
        {
            Container = implementation;
            _options = options;

            ExecutionExtensions
                .Try(() =>
                {
                    ConfigureAndVerify(typeProvider);
                })
                .Catch<Exception>()
                .Invoke(ex => throw new ContainerConfigurationException(ex).Rethrow());
        }

        internal IDependencyContainerImplementation Container { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            Container.Dispose();
        }

        #region Creation

        /// <summary>
        /// Creates IDependencyContainer without assembly limitations
        /// </summary>
        /// <param name="options">DependencyContainer creation options</param>
        /// <param name="implementationProducer">Dependency container implementation producer</param>
        /// <returns>DependencyContainer</returns>
        public static IDependencyContainer Create(
            DependencyContainerOptions options,
            Func<IDependencyContainerImplementation> implementationProducer)
        {
            var typeProvider = TypeProvider.CreateExactlyBounded(
                AssembliesExtensions.AllAssembliesFromCurrentDomain(),
                options.ExcludedAssemblies,
                options.ExcludedNamespaces);

            #pragma warning disable 618

            return new DependencyContainer(implementationProducer(), options, typeProvider);

            #pragma warning restore 618
        }

        /// <summary>
        /// Creates IDependencyContainer limited by specified ITypeProvider
        /// </summary>
        /// <param name="options">DependencyContainer creation options</param>
        /// <param name="implementationProducer">Dependency container implementation producer</param>
        /// <param name="typeProvider">ITypeProvider</param>
        /// <returns>DependencyContainer</returns>
        public static IDependencyContainer Create(
            DependencyContainerOptions options,
            Func<IDependencyContainerImplementation> implementationProducer,
            ITypeProvider typeProvider)
        {
#pragma warning disable 618

            return new DependencyContainer(implementationProducer(), options, typeProvider);

#pragma warning restore 618
        }

        /// <summary>
        /// Creates IDependencyContainer bounded above by specified assemblies
        /// </summary>
        /// <param name="options">DependencyContainer creation options</param>
        /// <param name="implementationProducer">Dependency container implementation producer</param>
        /// <param name="assemblies">Assembly for container configuration</param>
        /// <returns>DependencyContainer</returns>
        public static IDependencyContainer CreateBoundedAbove(
            DependencyContainerOptions options,
            Func<IDependencyContainerImplementation> implementationProducer,
            params Assembly[] assemblies)
        {
            var typeProvider = TypeProvider.CreateBoundedAbove(
                assemblies,
                options.ExcludedAssemblies,
                options.ExcludedNamespaces);

            #pragma warning disable 618

            return new DependencyContainer(implementationProducer(), options, typeProvider);

            #pragma warning restore 618
        }

        /// <summary>
        /// Creates IDependencyContainer exactly bounded by specified assemblies
        /// </summary>
        /// <param name="options">DependencyContainer creation options</param>
        /// <param name="implementationProducer">Dependency container implementation producer</param>
        /// <param name="assemblies">Assemblies for container configuration</param>
        /// <returns>DependencyContainer</returns>
        public static IDependencyContainer CreateExactlyBounded(
            DependencyContainerOptions options,
            Func<IDependencyContainerImplementation> implementationProducer,
            params Assembly[] assemblies)
        {
            var typeProvider = TypeProvider.CreateExactlyBounded(
                assemblies,
                options.ExcludedAssemblies,
                options.ExcludedNamespaces);

            #pragma warning disable 618

            return new DependencyContainer(implementationProducer(), options, typeProvider);

            #pragma warning restore 618
        }

        #endregion

        #region IScopedContainer

        /// <inheritdoc />
        public IDisposable OpenScope()
        {
            return Container.OpenScope();
        }

        #if NETSTANDARD2_1

        /// <inheritdoc />
        public IAsyncDisposable OpenScopeAsync()
        {
            return Container.OpenScopeAsync();
        }

        #endif

        #endregion

        #region IDependencyContainer

        /// <inheritdoc />
        public TService Resolve<TService>()
            where TService : class
        {
            return Resolve(typeof(TService),
                () =>
                {
                    IsNotInitializable(typeof(TService));
                    return Container.Resolve<TService>();
                });
        }

        /// <inheritdoc />
        public TService Resolve<TService, TRuntimeInfo>(TRuntimeInfo runtimeInfo)
            where TService : class, IInitializable<TRuntimeInfo>
        {
            return Resolve(typeof(TService),
                () =>
                {
                    var instance = Container.Resolve<TService>();

                    instance.Initialize(runtimeInfo);

                    return instance;
                });
        }

        /// <inheritdoc />
        public object Resolve(Type service)
        {
            return Resolve(service,
                () =>
                {
                    IsNotInitializable(service);
                    return Container.Resolve(service);
                });
        }

        /// <inheritdoc />
        public IEnumerable<TService> ResolveCollection<TService>()
            where TService : class
        {
            return Resolve(typeof(TService),
                () =>
                {
                    IsNotInitializable(typeof(TService));
                    return Container.ResolveCollection<TService>();
                });
        }

        /// <inheritdoc />
        public IEnumerable<object> ResolveCollection(Type service)
        {
            return Resolve(service, () =>
            {
                IsNotInitializable(service);
                return Container.ResolveCollection(service);
            });
        }

        #endregion

        #region Internals

        private void ConfigureAndVerify(ITypeProvider typeProvider)
        {
            var registrations = CollectRegistrations(typeProvider);

            Register(registrations);

            Container.ResolveCollection<IConfigurationVerifier>().Each(v => v.Verify());
        }

        private IRegistrationsContainer CollectRegistrations(ITypeProvider typeProvider)
        {
            var overrides = new ComponentsOverrideContainer(_options.ConstructorResolutionBehavior);
            var servicesProvider = new AutoRegistrationServicesProvider(typeProvider);
            var autoRegistrations = new AutoRegistrationsContainer(typeProvider, servicesProvider, _options.ConstructorResolutionBehavior);
            var manualRegistrations = new ManualRegistrationsContainer(typeProvider);

            var registrations = new CompositeRegistrationsContainer(
                overrides,
                autoRegistrations,
                manualRegistrations);

            _options = _options
                .WithManualRegistrations(new TypeProviderManualRegistration(typeProvider, servicesProvider))
                .WithManualRegistrations(new DependencyContainerManualRegistration(this, _options))
                .WithManualRegistrations(new RegistrationsContainerManualRegistration(registrations, overrides));

            _options.ManualRegistrations.Each(registration => registration.Register(manualRegistrations));
            _options.Overrides.Each(overrideInfo => overrideInfo.RegisterOverrides(overrides));

            return registrations;
        }

        private void Register(IRegistrationsContainer registrations)
        {
            registrations.Instances().RegisterInstances(Container);
            registrations.Resolvable().RegisterResolvable(Container);
            registrations.Delegates().RegisterDelegates(Container);
            registrations.Collections().RegisterCollections(Container);
            registrations.Decorators().RegisterDecorators(Container);
        }

        private static T Resolve<T>(Type service, Func<T> producer)
        {
            return producer
                .Try()
                .Catch<Exception>()
                .Invoke(ex => throw new ComponentResolutionException(service, ex).Rethrow());
        }

        private static void IsNotInitializable(Type serviceType)
        {
            if (serviceType.IsSubclassOfOpenGeneric(typeof(IInitializable<>)))
            {
                throw new InvalidOperationException($"Use overload of {nameof(IDependencyContainer.Resolve)} method with additional parameter to resolve initializable service {serviceType.FullName}");
            }
        }

        #endregion
    }
}