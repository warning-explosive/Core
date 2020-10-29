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
    using SimpleInjector;
    using SimpleInjector.Lifestyles;

    /// <summary>
    /// Dependency container implementation based on SimpleInjector
    /// Resolve dependencies by 'Dependency Injection' patterns
    /// Don't use it like ServiceLocator!
    /// </summary>
    [SuppressMessage("Regions", "SA1124", Justification = "Readability")]
    [ManualRegistration]
    internal class DependencyContainerImpl : IRegistrationContainer
    {
        private readonly ConcurrentDictionary<Type, Stack<VersionInfo>> _versions;

        private readonly Container _container;

        /// <summary> .ctor </summary>
        /// <param name="typeProvider">ITypeProvider</param>
        /// <param name="options">DependencyContainerOptions</param>
        public DependencyContainerImpl(ITypeProvider typeProvider,
                                       DependencyContainerOptions options)
        {
            _versions = new ConcurrentDictionary<Type, Stack<VersionInfo>>();
            _container = CreateContainer();

            var servicesProvider = new AutoWiringServicesProvider(typeProvider);

            RegisterSingletons(_container, this, (ContainerDependentTypeProvider)typeProvider, servicesProvider);

            RegisterAutoWired(_container, typeProvider, servicesProvider);

            RegisterExternalDependencies(this, options.RegistrationCallback);

            RegisterVersions(_container, typeProvider, servicesProvider);

            _container.Verify(VerificationOption.VerifyAndDiagnose);
            _container.GetAllInstances<IConfigurationVerifier>().Each(v => v.Verify());
        }

        #region IRegistrationContainer

        public IRegistrationContainer Register(Type serviceType, Type implementationType, EnLifestyle lifestyle)
        {
            _container.Register(serviceType, implementationType, lifestyle.MapLifestyle());
            return this;
        }

        public IRegistrationContainer Register<TService, TImplementation>(EnLifestyle lifestyle)
            where TService : class
            where TImplementation : class, TService
        {
            _container.Register<TService, TImplementation>(lifestyle.MapLifestyle());
            return this;
        }

        public IRegistrationContainer Register(Type serviceType, Func<object> factory, EnLifestyle lifestyle)
        {
            _container.Register(serviceType, factory.Invoke, lifestyle.MapLifestyle());
            return this;
        }

        public IRegistrationContainer Register<TService>(Func<TService> factory, EnLifestyle lifestyle)
            where TService : class
        {
            _container.Register(factory.Invoke, lifestyle.MapLifestyle());
            return this;
        }

        public IRegistrationContainer RegisterImplementation(Type implementationType, EnLifestyle lifestyle)
        {
            _container.Register(implementationType, implementationType, lifestyle.MapLifestyle());
            return this;
        }

        public IRegistrationContainer RegisterImplementation<TImplementation>(EnLifestyle lifestyle)
            where TImplementation : class
        {
            _container.Register<TImplementation>(lifestyle.MapLifestyle());
            return this;
        }

        public IRegistrationContainer RegisterCollection(Type serviceType, IEnumerable<object> implementationsCollection)
        {
            _container.Collection.Register(serviceType, implementationsCollection);
            return this;
        }

        public IRegistrationContainer RegisterCollection<TService>(IEnumerable<TService> implementationsCollection)
            where TService : class
        {
            _container.Collection.Register<TService>(implementationsCollection);
            return this;
        }

        public IRegistrationContainer RegisterVersioned<TService>(EnLifestyle lifestyle)
            where TService : class
        {
            return RegisterVersioned(typeof(TService), lifestyle);
        }

        public IRegistrationContainer RegisterVersioned(Type serviceType, EnLifestyle lifestyle)
        {
            _container.Register(typeof(IVersioned<>).MakeGenericType(serviceType),
                                typeof(Versioned<>).MakeGenericType(serviceType),
                                lifestyle.MapLifestyle());
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

        private static void RegisterExternalDependencies(IRegistrationContainer container, Action<IRegistrationContainer>? registrationCallback)
        {
            registrationCallback?.Invoke(container);
        }

        private static void RegisterAutoWired(Container container,
                                              ITypeProvider typeProvider,
                                              IAutoWiringServicesProvider servicesProvider)
        {
            /*
             * [I] - Single implementation service
             */
            var resolvableRegistrationInfos = servicesProvider
                                             .Resolvable()
                                             .GetComponents(container, typeProvider, false);
            container.RegisterServicesWithOpenGenericFallBack(resolvableRegistrationInfos);

            /*
             * [II] - External service single implementation
             */
            var externalServicesRegistrationInfos = servicesProvider
                                                   .External()
                                                   .GetComponents(container, typeProvider, false);
            container.RegisterServicesWithOpenGenericFallBack(externalServicesRegistrationInfos);

            /*
             * [III] - Collection resolvable service
             */
            var collectionResolvableRegistrationInfos = servicesProvider
                                                       .Collections()
                                                       .GetComponents(container, typeProvider, false)
                                                       .ToList();

            container.RegisterCollections(collectionResolvableRegistrationInfos);

            /*
             * [IV] - Decorators
             */
            var decorators = servicesProvider
                            .Decorators()
                            .Where(decorator => !decorator.IsSubclassOfOpenGeneric(typeof(IConditionalDecorator<,>)))
                            .GetDecoratorInfo(typeof(IDecorator<>));

            var conditionalDecorators = servicesProvider
                                       .Decorators()
                                       .Where(decorator => decorator.IsSubclassOfOpenGeneric(typeof(IConditionalDecorator<,>)))
                                       .GetConditionalDecoratorInfo(typeof(IConditionalDecorator<,>));

            decorators.Concat(conditionalDecorators)
                      .OrderByDependencyAttribute(z => z.ImplementationType)
                      .Each(container.RegisterDecorator);

            /*
             * [V] - Collection decorators
             */
            var collectionDecorators = servicesProvider
                                      .CollectionDecorators()
                                      .Where(decorator => !decorator.IsSubclassOfOpenGeneric(typeof(IConditionalCollectionDecorator<,>)))
                                      .GetDecoratorInfo(typeof(ICollectionDecorator<>));

            var collectionConditionalDecorators = servicesProvider
                                                 .CollectionDecorators()
                                                 .Where(decorator => decorator.IsSubclassOfOpenGeneric(typeof(IConditionalCollectionDecorator<,>)))
                                                 .GetConditionalDecoratorInfo(typeof(IConditionalCollectionDecorator<,>));

            collectionDecorators.Concat(collectionConditionalDecorators)
                                .OrderByDependencyAttribute(z => z.ImplementationType)
                                .Each(container.RegisterDecorator);
        }

        private static void RegisterVersions(Container container, ITypeProvider typeProvider, IAutoWiringServicesProvider servicesProvider)
        {
            var all = container
                .GetCurrentRegistrations()
                .Select(producer => producer.ServiceType)
                .ToList();

            // 1. IVersioned<T>
            container.RegisterVersionedComponents(all.VersionedComponents(container).ToList());

            // 2. IVersionFor<T> collection
            var versionFor = servicesProvider
                .Versions()
                .Select(service => typeof(IVersionFor<>).MakeGenericType(service))
                .GetComponents(container, typeProvider, true)
                .ToList();

            if (versionFor.Any())
            {
                container.RegisterCollections(versionFor);
            }
            else
            {
                container.RegisterEmptyCollection(typeof(IVersionFor<>));
            }
        }
    }
}