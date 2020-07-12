namespace SpaceEngineers.Core.AutoRegistration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
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
        private readonly Container _container;

        /// <summary> .ctor </summary>
        /// <param name="assemblies">All loaded assemblies</param>
        /// <param name="rootAssemblies">Root assemblies</param>
        /// <param name="registration">Register external dependencies</param>
        internal DependencyContainerImpl(Assembly[] assemblies,
                                         Assembly[] rootAssemblies,
                                         Action<IRegistrationContainer>? registration = null)
        {
            _container = CreateContainer();

            var typeExtensions = TypeExtensions.Configure(assemblies, rootAssemblies);
            var servicesProvider = new AutoWiringServicesProvider(_container, typeExtensions);

            RegisterSingletons(_container, this, typeExtensions, servicesProvider);

            RegisterAutoWired(_container, typeExtensions, servicesProvider);

            RegisterExternalDependencies(this, registration);

            // TODO: _container.Verify(VerificationOption.VerifyAndDiagnose);
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
            _container.Collection.Register(implementationsCollection);
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
                           EnableDynamicAssemblyCompilation = false,
                           SuppressLifestyleMismatchVerification = false,
                       }
                   };
        }

        private static void RegisterSingletons(Container container,
                                               IDependencyContainer dependencyContainer,
                                               ITypeExtensions typeExtensions,
                                               IAutoWiringServicesProvider servicesProvider)
        {
            container.RegisterSingleton(() => dependencyContainer);
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

            typeExtensions.OrderByDependencies(collectionResolvableRegistrationInfos, z => z.ComponentType)
                          .GroupBy(k => k.ServiceType, v => v.ComponentType)
                          .Each(info => container.Collection.Register(info.Key, info));

            /*
             * [IV] - Decorators
             */
            var decorators = Decorators(typeExtensions, typeof(IDecorator<>)).GetDecoratorInfo(typeof(IDecorator<>));
            var conditionalDecorators = Decorators(typeExtensions, typeof(IConditionalDecorator<,>)).GetConditionalDecoratorInfo(typeof(IConditionalDecorator<,>));

            typeExtensions.OrderByDependencies(decorators.Concat(conditionalDecorators), z => z.ComponentType)
                          .Each(container.RegisterDecorator);

            /*
             * [V] - Collection decorators
             */
            var collectionDecorators = Decorators(typeExtensions, typeof(ICollectionDecorator<>)).GetDecoratorInfo(typeof(ICollectionDecorator<>));
            var collectionConditionalDecorators = Decorators(typeExtensions, typeof(ICollectionConditionalDecorator<,>)).GetConditionalDecoratorInfo(typeof(ICollectionConditionalDecorator<,>));

            typeExtensions.OrderByDependencies(collectionDecorators.Concat(collectionConditionalDecorators), z => z.ComponentType)
                          .Each(container.RegisterDecorator);

            /*
             * [VI] - Concrete Implementations
             */
            var resolvableImplementations = autoWiringServicesProvider.Implementations().GetImplementationComponents(typeExtensions);
            container.RegisterImplementations(resolvableImplementations);
        }

        private static IEnumerable<Type> Decorators(ITypeExtensions typeExtensions, Type decoratorType)
        {
            return typeExtensions.OurTypes()
                                  .Where(t => !t.IsInterface
                                           && typeExtensions.IsSubclassOfOpenGeneric(t, decoratorType));
        }
    }
}