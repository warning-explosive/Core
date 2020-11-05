namespace SpaceEngineers.Core.AutoRegistration
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reactive.Disposables;
    using Abstractions;
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using AutoWiringApi.Services;
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
    internal class DependencyContainerImpl : IRegistrationContainer
    {
        private readonly ConcurrentDictionary<Type, Stack<VersionInfo>> _versions;

        private readonly Container _container;

        private readonly ICollection<ServiceRegistrationInfo> _external = new List<ServiceRegistrationInfo>();

        private readonly ICollection<ServiceRegistrationInfo> _externalCollections = new List<ServiceRegistrationInfo>();

        private readonly ICollection<ServiceRegistrationInfo> _externalVersioned = new List<ServiceRegistrationInfo>();

        /// <summary> .ctor </summary>
        /// <param name="typeProvider">ITypeProvider</param>
        /// <param name="servicesProvider">IAutoWiringServicesProvider</param>
        /// <param name="options">DependencyContainerOptions</param>
        public DependencyContainerImpl(ITypeProvider typeProvider,
                                       IAutoWiringServicesProvider servicesProvider,
                                       DependencyContainerOptions options)
        {
            _versions = new ConcurrentDictionary<Type, Stack<VersionInfo>>();
            _container = CreateContainer();

            RegisterSingletons(_container, this, (ContainerDependentTypeProvider)typeProvider, (AutoWiringServicesProvider)servicesProvider);

            options.RegistrationCallback?.Invoke(this);

            Resolvable(_container, typeProvider, servicesProvider).Concat(_external).ToList().RegisterServicesWithOpenGenericFallBack(_container);
            Collections(_container, typeProvider, servicesProvider).Concat(_externalCollections).ToList().RegisterCollections(_container);
            Decorators(servicesProvider).RegisterDecorators(_container);
            Versioned(_container).Concat(_externalVersioned).ToList().RegisterVersioned(_container);
            Versions(_container, typeProvider, servicesProvider).ToList().RegisterVersions(_container);

            _container.Verify(VerificationOption.VerifyAndDiagnose);
            _container.GetAllInstances<IConfigurationVerifier>().Each(v => v.Verify());
        }

        #region IRegistrationContainer

        public IRegistrationContainer Register(Type serviceType, Type implementationType, EnLifestyle lifestyle)
        {
            ServiceRegistrationInfoExtensions.ExternalComponents(serviceType, implementationType, lifestyle).Each(_external.Add);
            return this;
        }

        public IRegistrationContainer Register<TService, TImplementation>(EnLifestyle lifestyle)
            where TService : class
            where TImplementation : class, TService
        {
            return Register(typeof(TService), typeof(TImplementation), lifestyle);
        }

        public IRegistrationContainer Register(Type serviceType, Func<object> factory, EnLifestyle lifestyle)
        {
            _container.Register(serviceType, factory.Invoke, lifestyle.MapLifestyle());
            return this;
        }

        public IRegistrationContainer Register<TService>(Func<TService> factory, EnLifestyle lifestyle)
            where TService : class
        {
            return Register(typeof(TService), factory.Invoke, lifestyle);
        }

        public IRegistrationContainer RegisterCollection<TService>(IEnumerable<Type> implementations, EnLifestyle lifestyle)
            where TService : class
        {
            return RegisterCollection(typeof(TService), implementations, lifestyle);
        }

        public IRegistrationContainer RegisterCollection(Type serviceType, IEnumerable<Type> implementations, EnLifestyle lifestyle)
        {
            implementations.Select(implementation => new ServiceRegistrationInfo(serviceType, implementation, lifestyle))
                           .Each(_externalCollections.Add);
            return this;
        }

        public IRegistrationContainer RegisterVersioned<TService>(EnLifestyle lifestyle)
            where TService : class
        {
            return RegisterVersioned(typeof(TService), lifestyle);
        }

        public IRegistrationContainer RegisterVersioned(Type serviceType, EnLifestyle lifestyle)
        {
            _externalVersioned.Add(serviceType.VersionedComponent(lifestyle));
            return this;
        }

        public bool HasRegistration<TService>()
            where TService : class
        {
            return HasRegistration(typeof(TService));
        }

        public bool HasRegistration(Type serviceType)
        {
            return _container.GetCurrentRegistrations()
                             .Any(z => z.ServiceType == serviceType);
        }

        #endregion

        #region IScopedContainer

        public IDisposable OpenScope()
        {
            return AsyncScopedLifestyle.BeginScope(_container);
        }

#if NETSTANDARD2_1

        /// <summary>
        /// Open specified scope
        /// </summary>
        /// <returns>Scope cleanup</returns>
        public IAsyncDisposable OpenScopeAsync()
        {
            return AsyncScopedLifestyle.BeginScope(_container);
        }

#endif

        #endregion

        #region IDependencyContainer

        public T Resolve<T>()
            where T : class
        {
            return _container.GetInstance<T>();
        }

        public object Resolve(Type serviceType)
        {
            return _container.GetInstance(serviceType);
        }

        public IEnumerable<T> ResolveCollection<T>()
            where T : class
        {
            return _container.GetAllInstances<T>();
        }

        public IEnumerable<object> ResolveCollection(Type serviceType)
        {
            return _container.GetAllInstances(serviceType);
        }

        #endregion

        #region IVersionedContainer

        public IDisposable UseVersion<TService, TVersion>()
            where TService : class
            where TVersion : class, TService, IVersionFor<TService>
        {
            return UseVersionInternal(typeof(TService), typeof(TVersion), null);
        }

        public IDisposable UseVersion<TService>(Func<IVersionFor<TService>> versionFactory)
            where TService : class
        {
            return UseVersionInternal(typeof(TService), null, versionFactory);
        }

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
                                               DependencyContainerImpl dependencyContainer,
                                               ContainerDependentTypeProvider typeProvider,
                                               AutoWiringServicesProvider servicesProvider)
        {
            container.RegisterInstance<DependencyContainerImpl>(dependencyContainer);
            container.RegisterInstance<IRegistrationContainer>(dependencyContainer);
            container.RegisterInstance<IDependencyContainer>(dependencyContainer);
            container.RegisterInstance<IScopedContainer>(dependencyContainer);
            container.RegisterInstance<IVersionedContainer>(dependencyContainer);
            container.RegisterInstance<ContainerDependentTypeProvider>(typeProvider);
            container.RegisterInstance<ITypeProvider>(typeProvider);
            container.RegisterInstance<AutoWiringServicesProvider>(servicesProvider);
            container.RegisterInstance<IAutoWiringServicesProvider>(servicesProvider);
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