namespace SpaceEngineers.Core.AutoRegistration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using Abstractions;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using AutoWiring.Api.Services;
    using Basics;
    using Basics.Primitives;
    using Extensions;
    using Implementations;
    using Internals;
    using SimpleInjector;
    using SimpleInjector.Lifestyles;

    /// <summary>
    /// Dependency container implementation based on SimpleInjector
    /// Resolve dependencies by 'Dependency Injection' patterns
    /// Don't use it like ServiceLocator!
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    [SuppressMessage("Analysis", "CR1", Justification = "Registered by hand. See DependencyContainerImpl.")]
    [Component(EnLifestyle.Singleton, EnComponentRegistrationKind.ManuallyRegistered)]
    public class DependencyContainer : IDependencyContainer, IDisposable
    {
        private readonly Container _container;

        /// <summary> .cctor </summary>
        /// <param name="typeProvider">ContainerDependentTypeProvider</param>
        /// <param name="servicesProvider">AutoWiringServicesProvider</param>
        /// <param name="options">DependencyContainerOptions</param>
        [Obsolete("Use factory methods instead")]
        public DependencyContainer(ITypeProvider typeProvider,
                                   IAutoWiringServicesProvider servicesProvider,
                                   DependencyContainerOptions options)
        {
            _container = BuildContainer(typeProvider, servicesProvider, options);

            _container.Verify(VerificationOption.VerifyAndDiagnose);
            _container.GetAllInstances<IConfigurationVerifier>().Each(v => v.Verify());
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _container.Dispose();
        }

        #region Creation

        /// <summary>
        /// Creates default dependency container without assembly limitations
        /// Assemblies loaded in CurrentDomain from BaseDirectory and SpaceEngineers.Core.AutoWiring.Api assembly as root assembly
        /// </summary>
        /// <param name="options">DependencyContainer creation options</param>
        /// <returns>DependencyContainer</returns>
        public static IDependencyContainer Create(DependencyContainerOptions options)
        {
            return CreateExactlyBounded(options, AssembliesExtensions.AllAssembliesFromCurrentDomain());
        }

        /// <summary>
        /// Creates dependency container bounded above by specified assembly (project)
        /// </summary>
        /// <param name="options">DependencyContainer creation options</param>
        /// <param name="assemblies">Assembly for container configuration</param>
        /// <returns>DependencyContainer</returns>
        public static IDependencyContainer CreateBoundedAbove(DependencyContainerOptions options, params Assembly[] assemblies)
        {
            var belowAssemblies = assemblies
                .SelectMany(assembly => AssembliesExtensions.AllAssembliesFromCurrentDomain().Below(assembly))
                .Distinct()
                .ToArray();

            return CreateExactlyBounded(options, belowAssemblies);
        }

        /// <summary>
        /// Creates dependency container exactly bounded by specified assemblies
        /// </summary>
        /// <param name="options">DependencyContainer creation options</param>
        /// <param name="assemblies">Assemblies for container configuration</param>
        /// <returns>DependencyContainer</returns>
        public static IDependencyContainer CreateExactlyBounded(DependencyContainerOptions options, params Assembly[] assemblies)
        {
            var typeProvider = new TypeProvider(
                assemblies,
                RootAssemblies(),
                options.ExcludedAssemblies,
                options.ExcludedNamespaces);

            var servicesProvider = new AutoWiringServicesProvider(typeProvider);

            options = options.WithManualRegistrations(new TypeProviderManualRegistration(typeProvider));

#pragma warning disable 618
            using (var templateContainer = new DependencyContainer(typeProvider, servicesProvider, options))
            {
                return new DependencyContainer(
                templateContainer.Resolve<ITypeProvider>(),
                templateContainer.Resolve<IAutoWiringServicesProvider>(),
                options);
            }
#pragma warning restore 618
        }

        #endregion

        #region IScopedContainer

        /// <inheritdoc />
        public IDisposable OpenScope()
        {
            return AsyncScopedLifestyle.BeginScope(_container);
        }

#if NETSTANDARD2_1

        /// <inheritdoc />
        public IAsyncDisposable OpenScopeAsync()
        {
            return AsyncScopedLifestyle.BeginScope(_container);
        }

#endif

        #endregion

        #region IDependencyContainer

        /// <inheritdoc />
        public TService Resolve<TService>()
            where TService : class
        {
            VerifyService(typeof(TService));

            return _container.GetInstance<TService>();
        }

        /// <inheritdoc />
        public TService Resolve<TService, TRuntimeInfo>(TRuntimeInfo runtimeInfo)
            where TService : class, IInitializable<TRuntimeInfo>
        {
            var instance = _container.GetInstance<TService>();

            instance.Initialize(runtimeInfo);

            return instance;
        }

        /// <inheritdoc />
        public object Resolve(Type serviceType)
        {
            VerifyService(serviceType);

            return _container.GetInstance(serviceType);
        }

        /// <inheritdoc />
        public IEnumerable<TService> ResolveCollection<TService>()
            where TService : class
        {
            return _container.GetAllInstances<TService>();
        }

        /// <inheritdoc />
        public IEnumerable<object> ResolveCollection(Type serviceType)
        {
            return _container.GetAllInstances(serviceType);
        }

        #endregion

        #region Internals

        private static Assembly[] RootAssemblies()
        {
            return new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.AutoRegistration))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.AutoWiring), nameof(Core.AutoWiring.Api))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.Basics)))
            };
        }

        private static Container BuildSimpleInjector()
        {
            return new Container
            {
                Options =
                {
                    DefaultLifestyle = Lifestyle.Transient,
                    DefaultScopedLifestyle = new AsyncScopedLifestyle(),
                    UseFullyQualifiedTypeNames = true,
                    ResolveUnregisteredConcreteTypes = false,
                    AllowOverridingRegistrations = false,
                    SuppressLifestyleMismatchVerification = false,
                    UseStrictLifestyleMismatchBehavior = true,
                    EnableAutoVerification = true,
                }
            };
        }

        private Container BuildContainer(
            ITypeProvider typeProvider,
            IAutoWiringServicesProvider servicesProvider,
            DependencyContainerOptions options)
        {
            var container = BuildSimpleInjector();

            var registrationsContainer = CollectRegistrations(container, typeProvider, servicesProvider, options);
            var overridesContainer = CollectOverrides(options);

            ApplyRegistrations(container, registrationsContainer);
            ApplyOverrides(container, overridesContainer);

            return container;
        }

        private IRegistrationsContainer CollectRegistrations(
            Container container,
            ITypeProvider typeProvider,
            IAutoWiringServicesProvider servicesProvider,
            DependencyContainerOptions options)
        {
            var autoRegistrationsContainer = new AutoRegistrationsContainer(container, typeProvider, servicesProvider);

            var manualRegistrationsContainer = new ManualRegistrationsContainer();
            options
                .WithManualRegistrations(new DependencyContainerManualRegistration(this))
                .ManualRegistrations
                .Each(manual => manual.Register(manualRegistrationsContainer));

            return new CompositeRegistrationsContainer(autoRegistrationsContainer, manualRegistrationsContainer);
        }

        private static ManualRegistrationsContainer CollectOverrides(DependencyContainerOptions options)
        {
            var overridesContainer = new ManualRegistrationsContainer();
            options.Overrides.Each(manual => manual.Register(overridesContainer));

            return overridesContainer;
        }

        private static void ApplyRegistrations(Container container, IRegistrationsContainer registrationsContainer)
        {
            Register(container, registrationsContainer);
        }

        private static void ApplyOverrides(Container container, IRegistrationsContainer overridesContainer)
        {
            using (Disposable.Create(Allow(), Forbid))
            {
                Register(container, overridesContainer);
            }

            Container Allow()
            {
                container.Options.AllowOverridingRegistrations = true;
                return container;
            }

            void Forbid(Container c) => c.Options.AllowOverridingRegistrations = false;
        }

        private static void Register(Container container, IRegistrationsContainer registrationsContainer)
        {
            registrationsContainer.Singletons().RegisterSingletons(container);
            registrationsContainer.Resolvable().RegisterServicesWithOpenGenericFallBack(container);
            registrationsContainer.Delegates().RegisterDelegates(container);
            registrationsContainer.Collections().RegisterCollections(container);
            registrationsContainer.Decorators().RegisterDecorators(container);
        }

        private static void VerifyService(Type serviceType)
        {
            if (serviceType.IsSubclassOfOpenGeneric(typeof(IInitializable<>)))
            {
                throw new ActivationException($"Use overload of {nameof(IDependencyContainer.Resolve)} method with additional parameter to resolve initializable service {serviceType.FullName}");
            }
        }

        #endregion
    }
}