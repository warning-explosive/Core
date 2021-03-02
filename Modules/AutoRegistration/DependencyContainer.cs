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
    using AutoWiring.Api.Enumerations;
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
    public class DependencyContainer : IRegistrationContainer
    {
        private readonly ConcurrentDictionary<Type, Stack<VersionInfo>> _versions;

        private readonly Container _container;

        private readonly ICollection<ServiceRegistrationInfo> _external = new List<ServiceRegistrationInfo>();

        private readonly ICollection<ServiceRegistrationInfo> _externalCollections = new List<ServiceRegistrationInfo>();

        private readonly ICollection<Type> _emptyExternalCollections = new List<Type>();

        private readonly ICollection<ServiceRegistrationInfo> _externalVersioned = new List<ServiceRegistrationInfo>();

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
            _container = CreateContainer();

            options.ManualRegistrations.Each(manual => manual.Register(this));

            RegisterSingletons(_container, this, typeProvider, servicesProvider);
            Resolvable(_container, typeProvider, servicesProvider).Concat(_external).ToList().RegisterServicesWithOpenGenericFallBack(_container);
            Collections(_container, typeProvider, servicesProvider).Concat(_externalCollections).ToList().RegisterCollections(_container);
            _emptyExternalCollections.Each(collection => _container.RegisterEmptyCollection(collection));
            Decorators(servicesProvider).RegisterDecorators(_container);
            Versioned(_container).Concat(_externalVersioned).ToList().RegisterVersioned(_container);
            Versions(_container, typeProvider, servicesProvider).ToList().RegisterVersions(_container);

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

        #region IRegistrationContainer

        /// <inheritdoc />
        public IRegistrationContainer Register(Type serviceType, Type implementationType, EnLifestyle lifestyle)
        {
            _container.Register(serviceType, implementationType, lifestyle.MapLifestyle());
            return this;
        }

        /// <inheritdoc />
        public IRegistrationContainer Register<TService, TImplementation>(EnLifestyle lifestyle)
            where TService : class
            where TImplementation : class, TService
        {
            _container.Register<TService, TImplementation>(lifestyle.MapLifestyle());
            return this;
        }

        /// <inheritdoc />
        public IRegistrationContainer Register(Type serviceType, Func<object> factory, EnLifestyle lifestyle)
        {
            _container.Register(serviceType, factory.Invoke, lifestyle.MapLifestyle());
            return this;
        }

        /// <inheritdoc />
        public IRegistrationContainer Register<TService>(Func<TService> factory, EnLifestyle lifestyle)
            where TService : class
        {
            _container.Register<TService>(factory.Invoke, lifestyle.MapLifestyle());
            return this;
        }

        /// <inheritdoc />
        public IRegistrationContainer RegisterInstance<TService>(TService singletonInstance)
            where TService : class
        {
            _container.RegisterInstance<TService>(singletonInstance);
            return this;
        }

        /// <inheritdoc />
        public IRegistrationContainer RegisterInstance(Type serviceType, object singletonInstance)
        {
            _container.RegisterInstance(serviceType, singletonInstance);
            return this;
        }

        /// <inheritdoc />
        public IRegistrationContainer RegisterCollection<TService>(IEnumerable<Type> implementations, EnLifestyle lifestyle)
            where TService : class
        {
            return RegisterCollection(typeof(TService), implementations, lifestyle);
        }

        /// <inheritdoc />
        public IRegistrationContainer RegisterCollection(Type serviceType, IEnumerable<Type> implementations, EnLifestyle lifestyle)
        {
            implementations.Select(implementation => new ServiceRegistrationInfo(serviceType, implementation, lifestyle))
                           .Each(_externalCollections.Add);
            return this;
        }

        /// <inheritdoc />
        public IRegistrationContainer RegisterEmptyCollection<TService>()
            where TService : class
        {
            return RegisterEmptyCollection(typeof(TService));
        }

        /// <inheritdoc />
        public IRegistrationContainer RegisterEmptyCollection(Type serviceType)
        {
            _emptyExternalCollections.Add(serviceType);
            return this;
        }

        /// <inheritdoc />
        public IRegistrationContainer RegisterVersioned<TService>(EnLifestyle lifestyle)
            where TService : class
        {
            return RegisterVersioned(typeof(TService), lifestyle);
        }

        /// <inheritdoc />
        public IRegistrationContainer RegisterVersioned(Type serviceType, EnLifestyle lifestyle)
        {
            _externalVersioned.Add(serviceType.VersionedComponent(lifestyle));
            return this;
        }

        /// <inheritdoc />
        public bool HasRegistration<TService>()
            where TService : class
        {
            return HasRegistration(typeof(TService));
        }

        /// <inheritdoc />
        public bool HasRegistration(Type serviceType)
        {
            return _container.GetCurrentRegistrations()
                             .Any(z => z.ServiceType == serviceType);
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

        #region IVersionedContainer

        /// <inheritdoc />
        public IDisposable UseVersion<TService, TVersion>()
            where TService : class
            where TVersion : class, TService, IVersionFor<TService>
        {
            return UseVersionInternal(typeof(TService), typeof(TVersion), null);
        }

        /// <inheritdoc />
        public IDisposable UseVersion<TService>(Func<IVersionFor<TService>> versionFactory)
            where TService : class
        {
            return UseVersionInternal(typeof(TService), null, versionFactory);
        }

        /// <inheritdoc />
        public VersionInfo? AppliedVersion<TService>()
            where TService : class
        {
            _versions.TryGetValue(typeof(TService), out var stack);

            return stack?.Peek();
        }

        private IDisposable UseVersionInternal(Type serviceType, Type? versionType, Func<object>? versionInstanceFactory)
        {
            var stack = _versions.GetOrAdd(serviceType, _ => new Stack<VersionInfo>());
            stack.Push(new VersionInfo(serviceType, versionType, versionInstanceFactory));

            return Disposable.Create(_versions,
                                     state =>
                                     {
                                         if (state.TryGetValue(serviceType, out var s))
                                         {
                                             s.Pop();

                                             if (!s.Any())
                                             {
                                                 state.TryRemove(serviceType, out _);
                                             }
                                         }
                                     });
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

        private static void RegisterSingletons(Container container,
                                               DependencyContainer dependencyContainer,
                                               ITypeProvider typeProvider,
                                               IAutoWiringServicesProvider servicesProvider)
        {
            container.RegisterInstance<DependencyContainer>(dependencyContainer);
            container.RegisterInstance<IRegistrationContainer>(dependencyContainer);
            container.RegisterInstance<IDependencyContainer>(dependencyContainer);
            container.RegisterInstance<IScopedContainer>(dependencyContainer);
            container.RegisterInstance<IVersionedContainer>(dependencyContainer);
            container.RegisterInstance(typeProvider.GetType(), typeProvider);
            container.RegisterInstance<ITypeProvider>(typeProvider);
            container.RegisterInstance(servicesProvider.GetType(), servicesProvider);
            container.RegisterInstance<IAutoWiringServicesProvider>(servicesProvider);
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
            return servicesProvider.Resolvable().Concat(servicesProvider.External())
                                   .GetComponents(container, typeProvider, false);
        }

        private static IEnumerable<ServiceRegistrationInfo> Collections(
            Container container,
            ITypeProvider typeProvider,
            IAutoWiringServicesProvider servicesProvider)
        {
            return servicesProvider.Collections().GetComponents(container, typeProvider, false);
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

        private static IEnumerable<ServiceRegistrationInfo> Versioned(Container container)
        {
            return container
                  .GetCurrentRegistrations()
                  .OrderByComplexityDepth()
                  .Select(producer => producer.ServiceType)
                  .Where(serviceType => !serviceType.IsSubclassOfOpenGeneric(typeof(IInitializable<>)))
                  .VersionedComponents(container);
        }

        private static IEnumerable<ServiceRegistrationInfo> Versions(Container container, ITypeProvider typeProvider, IAutoWiringServicesProvider servicesProvider)
        {
            return servicesProvider
                  .Versions()
                  .Select(service => typeof(IVersionFor<>).MakeGenericType(service))
                  .GetComponents(container, typeProvider, true);
        }

        #endregion
    }
}