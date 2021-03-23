namespace SpaceEngineers.Core.AutoRegistration
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using Abstractions;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Services;
    using Basics;
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
    [SuppressMessage("Regions", "SA1124", Justification = "Readability")]
    [SuppressMessage("Analysis", "CR1", Justification = "Registered by hand. See DependencyContainerImpl.")]
    [ManualRegistration]
    public class DependencyContainer : IDependencyContainer
    {
        private readonly ConcurrentDictionary<Type, Stack<VersionInfo>> _versions;
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
            _versions = new ConcurrentDictionary<Type, Stack<VersionInfo>>();

            var manualRegistration = ManualRegistration(this, typeProvider, servicesProvider);
            options.ManualRegistrations = options.ManualRegistrations.Concat(new[] { manualRegistration }).ToList();

            _container = BuildContainer(typeProvider, servicesProvider, options);

            _container.Verify(VerificationOption.VerifyAndDiagnose);
            _container.GetAllInstances<IConfigurationVerifier>().Each(v => v.Verify());
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
            var typeProvider = new ContainerDependentTypeProvider(
                AssembliesExtensions.AllFromCurrentDomain(),
                RootAssemblies(),
                options.ExcludedAssemblies,
                options.ExcludedNamespaces);

            var servicesProvider = new AutoWiringServicesProvider(typeProvider);

#pragma warning disable 618
            return new DependencyContainer(typeProvider, servicesProvider, options);
#pragma warning restore 618
        }

        /// <summary>
        /// Creates dependency container exactly bounded by specified assemblies
        /// </summary>
        /// <param name="assemblies">Assemblies for container configuration</param>
        /// <param name="options">DependencyContainer creation options</param>
        /// <returns>DependencyContainer</returns>
        public static IDependencyContainer CreateExactlyBounded(Assembly[] assemblies, DependencyContainerOptions options)
        {
            var typeProvider = new ContainerDependentTypeProvider(
                assemblies,
                RootAssemblies(),
                options.ExcludedAssemblies,
                options.ExcludedNamespaces);

            var servicesProvider = new AutoWiringServicesProvider(typeProvider);

#pragma warning disable 618
            return new DependencyContainer(typeProvider, servicesProvider, options);
#pragma warning restore 618
        }

        /// <summary>
        /// Creates dependency container bounded above by specified assembly (project)
        /// </summary>
        /// <param name="assembly">Assembly for container configuration</param>
        /// <param name="options">DependencyContainer creation options</param>
        /// <returns>DependencyContainer</returns>
        public static IDependencyContainer CreateBoundedAbove(Assembly assembly, DependencyContainerOptions options)
        {
            var typeProvider = new ContainerDependentTypeProvider(
                AssembliesExtensions.AllFromCurrentDomain().Below(assembly),
                RootAssemblies(),
                options.ExcludedAssemblies,
                options.ExcludedNamespaces);

            var servicesProvider = new AutoWiringServicesProvider(typeProvider);

#pragma warning disable 618
            return new DependencyContainer(typeProvider, servicesProvider, options);
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
            var regularRootAssemblies = new[]
            {
                typeof(DependencyContainer).Assembly, // AutoRegistration
                typeof(LifestyleAttribute).Assembly, // AutoWiring.Api
                typeof(TypeExtensions).Assembly // Basics
            };

            return regularRootAssemblies.Concat(OptionalRootAssemblies()).ToArray();
        }

        private static IEnumerable<Assembly> OptionalRootAssemblies()
        {
            const string optionalAssemblyName = "SpaceEngineers.Core.GenericEndpoint.Contract";
            var genericEndpointContract = AssembliesExtensions.FindAssembly(optionalAssemblyName);

            return genericEndpointContract != null
                ? new[] { genericEndpointContract }
                : Array.Empty<Assembly>();
        }

        private static Container BuildContainer(
            ITypeProvider typeProvider,
            IAutoWiringServicesProvider servicesProvider,
            DependencyContainerOptions options)
        {
            var container = CreateContainer();

            var manualRegistrationsContainer = new ManualRegistrationsContainer();

            options.ManualRegistrations.Each(manual => manual.Register(manualRegistrationsContainer));

            manualRegistrationsContainer.Singletons().RegisterSingletons(container);
            Resolvable(container, typeProvider, servicesProvider).Concat(manualRegistrationsContainer.Resolvable()).RegisterServicesWithOpenGenericFallBack(container);
            Collections(container, typeProvider, servicesProvider).Concat(manualRegistrationsContainer.Collections()).ToList().RegisterCollections(container);
            Decorators(servicesProvider).Concat(manualRegistrationsContainer.Decorators()).RegisterDecorators(container);

            return container;
        }

        private static Container CreateContainer()
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
                           EnableAutoVerification = true
                       }
                   };
        }

        private static IManualRegistration ManualRegistration(
            DependencyContainer dependencyContainer,
            ITypeProvider typeProvider,
            IAutoWiringServicesProvider servicesProvider)
        {
            return DependencyContainerOptions
                .DelegateRegistration(container =>
                {
                    container
                        .RegisterInstance<DependencyContainer>(dependencyContainer)
                        .RegisterInstance<IDependencyContainer>(dependencyContainer)
                        .RegisterInstance<IScopedContainer>(dependencyContainer)
                        .RegisterInstance(typeProvider.GetType(), typeProvider)
                        .RegisterInstance<ITypeProvider>(typeProvider)
                        .RegisterInstance(servicesProvider.GetType(), servicesProvider)
                        .RegisterInstance<IAutoWiringServicesProvider>(servicesProvider);
                });
        }

        private static void VerifyService(Type serviceType)
        {
            if (serviceType.IsSubclassOfOpenGeneric(typeof(IInitializable<>)))
            {
                throw new InvalidOperationException($"Use overload with additional parameter to resolve initializable service {serviceType.FullName}");
            }
        }

        private static IEnumerable<ServiceRegistrationInfo> Resolvable(
            Container container,
            ITypeProvider typeProvider,
            IAutoWiringServicesProvider servicesProvider)
        {
            return servicesProvider
                .Resolvable()
                .Concat(servicesProvider.External())
                .GetComponents(container, typeProvider);
        }

        private static IEnumerable<ServiceRegistrationInfo> Collections(
            Container container,
            ITypeProvider typeProvider,
            IAutoWiringServicesProvider servicesProvider)
        {
            return servicesProvider
                .Collections()
                .GetComponents(container, typeProvider);
        }

        private static IEnumerable<DecoratorRegistrationInfo> Decorators(IAutoWiringServicesProvider servicesProvider)
        {
            var decorators = servicesProvider
                .Decorators()
                .Where(decorator => !decorator.IsSubclassOfOpenGeneric(typeof(IConditionalDecorator<,>)))
                .GetDecoratorInfo(typeof(IDecorator<>));

            var conditionalDecorators = servicesProvider
                .Decorators()
                .Where(decorator => decorator.IsSubclassOfOpenGeneric(typeof(IConditionalDecorator<,>)))
                .GetConditionalDecoratorInfo(typeof(IConditionalDecorator<,>));

            var collectionDecorators = servicesProvider
                .CollectionDecorators()
                .Where(decorator => !decorator.IsSubclassOfOpenGeneric(typeof(IConditionalCollectionDecorator<,>)))
                .GetDecoratorInfo(typeof(ICollectionDecorator<>));

            var collectionConditionalDecorators = servicesProvider
                .CollectionDecorators()
                .Where(decorator => decorator.IsSubclassOfOpenGeneric(typeof(IConditionalCollectionDecorator<,>)))
                .GetConditionalDecoratorInfo(typeof(IConditionalCollectionDecorator<,>));

            var simple = decorators.Concat(collectionDecorators);
            var conditional = conditionalDecorators.Concat(collectionConditionalDecorators);
            return simple.Concat(conditional);
        }

        #endregion
    }
}