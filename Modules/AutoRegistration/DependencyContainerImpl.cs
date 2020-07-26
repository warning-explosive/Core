namespace SpaceEngineers.Core.AutoRegistration
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reactive.Disposables;
    using System.Reflection;
    using Abstractions;
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Basics;
    using Internals;
    using SimpleInjector;
    using SimpleInjector.Lifestyles;

    /// <summary>
    /// Dependency container implementation based on SimpleInjector
    /// Resolve dependencies by 'Dependency Injection' patterns
    /// Don't use it like ServiceLocator!
    /// </summary>
    [SuppressMessage("Regions", "SA1124", Justification = "Readability")]
    [Unregistered]
    internal class DependencyContainerImpl : IRegistrationContainer
    {
        private readonly ConcurrentDictionary<Type, Type> _versions;

        private readonly Container _container;

        /// <summary> .ctor </summary>
        /// <param name="typeExtensions">ITypeExtensions</param>
        /// <param name="options">DependencyContainerOptions</param>
        internal DependencyContainerImpl(ITypeExtensions typeExtensions,
                                         DependencyContainerOptions options)
        {
            _versions = new ConcurrentDictionary<Type, Type>();
            _container = CreateContainer();

            var servicesProvider = new AutoWiringServicesProvider(_container, typeExtensions);

            RegisterSingletons(_container, this, typeExtensions, servicesProvider);

            RegisterAutoWired(_container, typeExtensions, servicesProvider);

            RegisterExternalDependencies(this, options.RegistrationCallback);

            RegisterVersions(_container, servicesProvider);

            _container.Verify(VerificationOption.VerifyAndDiagnose);
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

        public IRegistrationContainer RegisterConcrete(Type concreteType, EnLifestyle lifestyle)
        {
            _container.Register(concreteType, concreteType, lifestyle.MapLifestyle());
            return this;
        }

        public IRegistrationContainer RegisterConcrete<TConcrete>(EnLifestyle lifestyle)
            where TConcrete : class
        {
            _container.Register<TConcrete>(lifestyle.MapLifestyle());
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

        public IDisposable UseVersion<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            _versions.GetOrAdd(typeof(TService), () => typeof(TImplementation));

            return Disposable.Create(_versions, state => state.TryRemove(typeof(TService), out _));
        }

        public Type? AppliedVersion<TService>()
            where TService : class
        {
            _versions.TryGetValue(typeof(TService), out var version);

            return version;
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
                       }
                   };
        }

        private static void RegisterSingletons(Container container,
                                               DependencyContainerImpl dependencyContainer,
                                               ITypeExtensions typeExtensions,
                                               IAutoWiringServicesProvider servicesProvider)
        {
            container.RegisterSingleton<IDependencyContainer>(() => dependencyContainer);
            container.RegisterSingleton<IRegistrationContainer>(() => dependencyContainer);
            container.RegisterSingleton<IScopedContainer>(() => dependencyContainer);
            container.RegisterSingleton<IVersionedContainer>(() => dependencyContainer);
            container.RegisterSingleton(() => typeExtensions);
            container.RegisterSingleton(() => servicesProvider);
        }

        private static void RegisterExternalDependencies(IRegistrationContainer container, Action<IRegistrationContainer>? registrationCallback)
        {
            registrationCallback?.Invoke(container);
        }

        private static void RegisterAutoWired(Container container,
                                              ITypeExtensions typeExtensions,
                                              IAutoWiringServicesProvider autoWiringServicesProvider)
        {
            /*
             * [I] - Single implementation service
             */
            var resolvableRegistrationInfos = autoWiringServicesProvider.Resolvable().GetComponents(container, typeExtensions);
            container.RegisterWithOpenGenericFallBack(resolvableRegistrationInfos);

            /*
             * [II] - External service single implementation
             */
            var externalServicesRegistrationInfos = autoWiringServicesProvider.External().GetComponents(container, typeExtensions);
            container.RegisterWithOpenGenericFallBack(externalServicesRegistrationInfos);

            /*
             * [III] - Collection resolvable service
             */
            var collectionResolvableRegistrationInfos = autoWiringServicesProvider.Collections().GetComponents(container, typeExtensions);

            // register each element of collection as implementation to provide lifestyle for container
            container.RegisterImplementations(collectionResolvableRegistrationInfos);
            container.RegisterCollections(collectionResolvableRegistrationInfos);

            /*
             * [IV] - Decorators
             */
            var decorators = Decorators(typeExtensions, typeof(IDecorator<>)).GetDecoratorInfo(typeof(IDecorator<>));
            var conditionalDecorators = Decorators(typeExtensions, typeof(IConditionalDecorator<,>)).GetConditionalDecoratorInfo(typeof(IConditionalDecorator<,>));

            decorators.Concat(conditionalDecorators)
                      .OrderByDependencies(z => z.ComponentType)
                      .Each(container.RegisterDecorator);

            /*
             * [V] - Collection decorators
             */
            var collectionDecorators = Decorators(typeExtensions, typeof(ICollectionDecorator<>)).GetDecoratorInfo(typeof(ICollectionDecorator<>));
            var collectionConditionalDecorators = Decorators(typeExtensions, typeof(ICollectionConditionalDecorator<,>)).GetConditionalDecoratorInfo(typeof(ICollectionConditionalDecorator<,>));

            collectionDecorators.Concat(collectionConditionalDecorators)
                                .OrderByDependencies(z => z.ComponentType)
                                .Each(container.RegisterDecorator);

            /*
             * [VI] - Concrete Implementations
             */
            var resolvableImplementations = autoWiringServicesProvider.Implementations().GetImplementationComponents();
            container.RegisterImplementations(resolvableImplementations);
        }

        private static IEnumerable<Type> Decorators(ITypeExtensions typeExtensions, Type decoratorType)
        {
            return typeExtensions.OurTypes()
                                 .Where(t => t.IsClass
                                          && !t.IsInterface
                                          && !t.IsAbstract
                                          && t.IsSubclassOfOpenGeneric(decoratorType));
        }

        private static void RegisterVersions(Container container, IAutoWiringServicesProvider autoWiringServicesProvider)
        {
            var versions = autoWiringServicesProvider.Versions().VersionComponents(container);

            container.RegisterWithOpenGenericFallBack(versions);
        }
    }
}