namespace SpaceEngineers.Core.CompositionRoot
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using Abstractions;
    using Api.Abstractions;
    using Api.Exceptions;
    using AutoRegistration.Api.Abstractions;
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
    [SuppressMessage("Analysis", "CR1", Justification = "Registered by hand. See DependencyContainerImpl.")]
    [ManuallyRegisteredComponent]
    public class DependencyContainer : IDependencyContainer, IDisposable
    {
        private DependencyContainerOptions _options;

        /// <summary> .cctor </summary>
        /// <param name="implementation">IDependencyContainerImplementation</param>
        /// <param name="typeProvider">ContainerDependentTypeProvider</param>
        /// <param name="servicesProvider">AutoWiringServicesProvider</param>
        /// <param name="options">DependencyContainerOptions</param>
        [Obsolete("Use factory methods instead")]
        public DependencyContainer(
            IDependencyContainerImplementation implementation,
            DependencyContainerOptions options,
            ITypeProvider typeProvider,
            IAutoRegistrationServicesProvider servicesProvider)
        {
            Container = implementation;
            _options = options;

            ExecutionExtensions
                .Try(() =>
                {
                    ConfigureAndVerify(typeProvider, servicesProvider);
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
        /// Creates default dependency container without assembly limitations
        /// Assemblies loaded in CurrentDomain from BaseDirectory and SpaceEngineers.Core.AutoRegistration.Api assembly as root assembly
        /// </summary>
        /// <param name="options">DependencyContainer creation options</param>
        /// <param name="implementationProducer">Dependency container implementation producer</param>
        /// <returns>DependencyContainer</returns>
        public static IDependencyContainer Create(
            DependencyContainerOptions options,
            Func<IDependencyContainerImplementation> implementationProducer)
        {
            return CreateExactlyBounded(
                options,
                implementationProducer,
                AssembliesExtensions.AllAssembliesFromCurrentDomain());
        }

        /// <summary>
        /// Creates dependency container bounded above by specified assembly (project)
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
            var belowAssemblies = assemblies
                .SelectMany(assembly => AssembliesExtensions.AllAssembliesFromCurrentDomain().Below(assembly))
                .Distinct()
                .ToArray();

            return CreateExactlyBounded(options, implementationProducer, belowAssemblies);
        }

        /// <summary>
        /// Creates dependency container exactly bounded by specified assemblies
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
            var typeProvider = new TypeProvider(
                assemblies,
                RootAssemblies(),
                options.ExcludedAssemblies,
                options.ExcludedNamespaces);

            #pragma warning disable 618

            using var templateContainer = new DependencyContainer(
                implementationProducer(),
                options.WithManualRegistrations(new TypeProviderManualRegistration(typeProvider)),
                typeProvider,
                new AutoRegistrationServicesProvider(typeProvider));

            return new DependencyContainer(
                implementationProducer(),
                options.WithManualRegistrations(new TypeProviderManualRegistration(typeProvider)),
                templateContainer.Resolve<ITypeProvider>(),
                templateContainer.Resolve<IAutoRegistrationServicesProvider>());

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

        private static Assembly[] RootAssemblies()
        {
            return new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.Basics))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.AutoRegistration), nameof(Core.AutoRegistration.Api))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.CompositionRoot)))
            };
        }

        private void ConfigureAndVerify(ITypeProvider typeProvider, IAutoRegistrationServicesProvider servicesProvider)
        {
            var registrations = CollectRegistrations(typeProvider, servicesProvider);

            Register(registrations);

            Container.ResolveCollection<IConfigurationVerifier>().Each(v => v.Verify());
        }

        private IRegistrationsContainer CollectRegistrations(
            ITypeProvider typeProvider,
            IAutoRegistrationServicesProvider servicesProvider)
        {
            var overrides = CollectOverrides();

            var manualRegistrations = new ManualRegistrationsContainer();

            IRegistrationsContainer registrations;

            if (_options.UseAutoRegistration)
            {
                var autoRegistrations = new AutoRegistrationsContainer(typeProvider, servicesProvider, _options.ConstructorResolutionBehavior);
                registrations = new CompositeRegistrationsContainer(overrides, autoRegistrations, manualRegistrations);
            }
            else
            {
                registrations = new CompositeRegistrationsContainer(overrides, manualRegistrations);
            }

            _options = _options
                .WithManualRegistrations(new DependencyContainerManualRegistration(this, _options))
                .WithManualRegistrations(new RegistrationsContainerManualRegistration(registrations));

            _options.ManualRegistrations.Each(registration => registration.Register(manualRegistrations));

            return registrations;
        }

        private IRegistrationsContainer CollectOverrides()
        {
            var overrides = new ManualRegistrationsContainer();
            _options.Overrides.Each(manual => manual.Register(overrides));

            return overrides;
        }

        private void Register(IRegistrationsContainer registrations)
        {
            registrations.Singletons().RegisterSingletons(Container);
            registrations.Resolvable().RegisterServicesWithOpenGenericFallBack(Container);
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