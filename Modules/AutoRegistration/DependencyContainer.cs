namespace SpaceEngineers.Core.AutoRegistration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using Basics;
    using SimpleInjector;

    /// <summary>
    /// Dependency container
    /// Resolve dependencies by 'Dependency Injection' patterns
    /// Don't use it like ServiceLocator!
    /// </summary>
    [ManualRegistration]
    public class DependencyContainer : IDependencyContainer,
                                       IResolvableImplementation
    {
        private readonly Container _container;

        /// <summary> .ctor </summary>
        /// <param name="assemblies">All loaded assemblies</param>
        /// <param name="rootAssemblies">Root assemblies</param>
        public DependencyContainer(Assembly[] assemblies, Assembly[] rootAssemblies)
        {
            _container = InitContainer(assemblies, rootAssemblies, this);
        }

        /// <summary>
        /// Default configured container
        /// Assemblies loaded in CurrentDomain and SpaceEngineers.Core.AutoWiringApi assembly as root assembly
        /// </summary>
        /// <param name="entryPointAssembly">Entry point assembly, should contains all references - application root</param>
        /// <returns>DependencyContainer</returns>
        public static IDependencyContainer Default(Assembly entryPointAssembly)
        {
            AssembliesExtensions.WarmUpAppDomain(entryPointAssembly);

            var autoWiringApi = typeof(LifestyleAttribute).Assembly;

            return new DependencyContainer(AssembliesExtensions.AllFromCurrentDomain(),
                                           new[] { autoWiringApi });
        }

        /// <inheritdoc />
        public T Resolve<T>()
            where T : class, IResolvable
        {
            return _container.GetInstance<T>();
        }

        /// <inheritdoc />
        public object Resolve(Type serviceType)
        {
            if (!serviceType.IsInterface
                || serviceType.IsGenericTypeDefinition
                || !typeof(IResolvable).IsAssignableFrom(serviceType))
            {
                throw new ArgumentException($"{serviceType.FullName} must be an closed-generic or non-generic interface and derived from {nameof(IResolvable)}");
            }

            return _container.GetInstance(serviceType);
        }

        /// <inheritdoc />
        public T ResolveImplementation<T>()
            where T : class, IResolvableImplementation
        {
            return _container.GetInstance<T>();
        }

        /// <inheritdoc />
        public object ResolveImplementation(Type concreteImplementationType)
        {
            if (!concreteImplementationType.IsClass
                || concreteImplementationType.IsAbstract
                || concreteImplementationType.IsGenericTypeDefinition
                || !typeof(IResolvableImplementation).IsAssignableFrom(concreteImplementationType))
            {
                throw new ArgumentException($"{concreteImplementationType.FullName} must be an closed-generic or non-generic concrete (non-abstract) type and derived from {nameof(IResolvableImplementation)}");
            }

            return _container.GetInstance(concreteImplementationType);
        }

        /// <inheritdoc />
        public IEnumerable<T> ResolveCollection<T>()
            where T : class, ICollectionResolvable
        {
            return _container.GetAllInstances<T>();
        }

        /// <inheritdoc />
        public IEnumerable<object> ResolveCollection(Type serviceType)
        {
            if (!serviceType.IsInterface
                || serviceType.IsGenericTypeDefinition
                || !typeof(ICollectionResolvable).IsAssignableFrom(serviceType))
            {
                throw new ArgumentException($"{serviceType.FullName} must be an closed-generic or non-generic interface and derived from {nameof(ICollectionResolvable)}");
            }

            return _container.GetAllInstances(serviceType);
        }

        /// <inheritdoc />
        public TExternalService ResolveExternal<TExternalService>()
            where TExternalService : class
        {
            return _container.GetInstance<TExternalService>();
        }

        /// <inheritdoc />
        public object ResolveExternal(Type externalServiceType)
        {
            if (!externalServiceType.IsInterface
                || externalServiceType.IsGenericTypeDefinition)
            {
                throw new ArgumentException($"{externalServiceType.FullName} must be an closed-generic or non-generic interface");
            }

            return _container.GetInstance(externalServiceType);
        }

        private static Container InitContainer(Assembly[] assemblies,
                                               Assembly[] rootAssemblies,
                                               DependencyContainer dependencyContainer)
        {
            var typeExtensions = TypeExtensions.Configure(assemblies, rootAssemblies);

            var container = new Container
                            {
                                Options =
                                {
                                    DefaultLifestyle = Lifestyle.Transient,
                                    UseFullyQualifiedTypeNames = true,
                                    ResolveUnregisteredConcreteTypes = false,
                                    AllowOverridingRegistrations = false,
                                    EnableDynamicAssemblyCompilation = false,
                                    SuppressLifestyleMismatchVerification = false,
                                },
                            };

            RegisterDependencyContainer(container, dependencyContainer);

            RegisterServices(container, typeExtensions);

            container.Verify(VerificationOption.VerifyAndDiagnose);

            return container;
        }

        private static void RegisterDependencyContainer(Container container, DependencyContainer dependencyContainer)
        {
            container.RegisterSingleton(() => dependencyContainer);
        }

        private static void RegisterServices(Container container, ITypeExtensions typeExtensions)
        {
            /*
             * [I] - Single implementation service
             */
            var resolvableRegistrationInfos = GetServiceRegistrationInfo<IResolvable>(container, typeExtensions);

            foreach (var resolvable in resolvableRegistrationInfos)
            {
                container.Register(resolvable.ServiceType, resolvable.ComponentType, resolvable.Lifestyle);
            }

            /*
             * [II] - External service single implementation
             */
            var externalServicesRegistrationInfos = GetExternalServiceRegistrationInfo(container, typeExtensions);

            foreach (var resolvable in externalServicesRegistrationInfos)
            {
                container.Register(resolvable.ServiceType, resolvable.ComponentType, resolvable.Lifestyle);
            }

            /*
             * [III] - Collection resolvable service
             */
            var collectionResolvableRegistrationInfos = GetServiceRegistrationInfo<ICollectionResolvable>(container, typeExtensions);

            // register each element of collection as implementation to provide lifestyle for container
            RegisterImplementations(container, collectionResolvableRegistrationInfos);

            typeExtensions.OrderByDependencies(collectionResolvableRegistrationInfos, z => z.ComponentType)
                          .GroupBy(k => k.ServiceType, v => v.ComponentType)
                          .Each(info => container.Collection.Register(info.Key, info));

            /*
             * [IV] - Decorators
             */
            var decoratorInfos = GetDecoratorInfo(typeExtensions, typeof(IDecorator<>))
               .Concat(GetConditionalDecoratorInfo(typeExtensions, typeof(IConditionalDecorator<,>)));

            typeExtensions.OrderByDependencies(decoratorInfos, z => z.ComponentType)
                          .Each(info => RegisterDecorator(container, info));

            /*
             * [V] - Collection decorators
             */
            var collectionDecoratorInfos = GetDecoratorInfo(typeExtensions, typeof(ICollectionDecorator<>))
               .Concat(GetConditionalDecoratorInfo(typeExtensions, typeof(ICollectionConditionalDecorator<,>)));

            typeExtensions.OrderByDependencies(collectionDecoratorInfos, z => z.ComponentType)
                          .Each(info => RegisterDecorator(container, info));

            /*
             * [VI] - Concrete Implementations
             */
            var resolvableImplementations = GetImplementationRegistrationInfo(container, typeExtensions);
            RegisterImplementations(container, resolvableImplementations);
        }

        private static void RegisterImplementations(Container container,
                                                    IEnumerable<ServiceRegistrationInfo> serviceRegistrationInfos)
        {
            foreach (var info in serviceRegistrationInfos)
            {
                container.Register(info.ComponentType, info.ComponentType, info.Lifestyle);
            }
        }

        private static IEnumerable<ServiceRegistrationInfo> GetComponents(Container container,
                                                                          ITypeExtensions typeExtensions,
                                                                          Type serviceType)
        {
            return container.GetTypesToRegister(serviceType,
                                                typeExtensions.OurAssemblies(),
                                                new TypesToRegisterOptions
                                                {
                                                    IncludeComposites = false,
                                                    IncludeDecorators = false,
                                                    IncludeGenericTypeDefinitions = true
                                                })
                            .Where(ForAutoRegistration)
                            .Select(cmp => new ServiceRegistrationInfo(serviceType, cmp, cmp.GetCustomAttribute<LifestyleAttribute>()?.Lifestyle));
        }

        private static ServiceRegistrationInfo[] GetServiceRegistrationInfo<TInterface>(Container container,
                                                                                        ITypeExtensions typeExtensions)
            where TInterface : class
        {
            return typeExtensions
                  .AllOurServicesThatContainsDeclarationOfInterface<TInterface>()
                  .SelectMany(i => GetComponents(container, typeExtensions, i))
                  .ToArray();
        }

        private static ServiceRegistrationInfo[] GetExternalServiceRegistrationInfo(Container container,
                                                                                    ITypeExtensions typeExtensions)
        {
            return typeExtensions
                  .AllLoadedTypes()
                  .Where(type => type.IsClass
                                 && !type.IsAbstract
                                 && typeExtensions.IsSubclassOfOpenGeneric(type, typeof(IExternalResolvable<>)))
                  .SelectMany(type => typeExtensions.GetGenericArgumentsOfOpenGenericAt(type, typeof(IExternalResolvable<>), 0))
                  .SelectMany(i => GetComponents(container, typeExtensions, i))
                  .ToArray();
        }

        private static ServiceRegistrationInfo[] GetImplementationRegistrationInfo(Container container,
                                                                                   ITypeExtensions typeExtensions)
        {
            var implementationType = typeof(IResolvableImplementation);

            return GetComponents(container, typeExtensions, implementationType).ToArray();
        }

        private static IEnumerable<ServiceRegistrationInfo> GetDecoratorInfo(ITypeExtensions typeExtensions,
                                                                             Type decoratorType)
        {
            return GetGenericDecoratorInfo(typeExtensions, decoratorType).Select(pair => pair.Info);
        }

        private static IEnumerable<ServiceRegistrationInfo> GetConditionalDecoratorInfo(ITypeExtensions typeExtensions,
                                                                                        Type decoratorType)
        {
            return GetGenericDecoratorInfo(typeExtensions, decoratorType)
                  .Select(pair =>
                          {
                              pair.Info.Attribute = pair.Decorator.GetGenericArguments()[1];
                              return pair.Info;
                          });
        }

        private static IEnumerable<(Type Decorator, ServiceRegistrationInfo Info)> GetGenericDecoratorInfo(ITypeExtensions typeExtensions,
                                                                                                           Type decoratorType)
        {
            return typeExtensions
                  .OurTypes()
                  .Where(t => !t.IsInterface
                           && typeExtensions.IsSubclassOfOpenGeneric(t, decoratorType))
                  .Where(ForAutoRegistration)
                  .Select(t => new
                               {
                                   ComponentType = t,
                                   Decorator = ExtractDecorator(decoratorType, t),
                                   t.GetCustomAttribute<LifestyleAttribute>()?.Lifestyle
                               })
                  .Select(t => (Decrator: t.Decorator, new ServiceRegistrationInfo(t.Decorator.GetGenericArguments()[0], t.ComponentType, t.Lifestyle)));
        }

        private static Type ExtractDecorator(Type decoratorType, Type t)
        {
            return t.GetInterfaces()
                    .Where(i => i.IsGenericType)
                    .Single(i => i.GetGenericTypeDefinition() == decoratorType);
        }

        private static void RegisterDecorator(Container container, ServiceRegistrationInfo info)
        {
            if (info.Attribute == null)
            {
                container.RegisterDecorator(info.ServiceType,
                                            info.ComponentType,
                                            info.Lifestyle);
            }
            else
            {
                container.RegisterDecorator(info.ServiceType,
                                            info.ComponentType,
                                            info.Lifestyle,
                                            c => c.ImplementationType.GetCustomAttribute(info.Attribute) != null);
            }
        }

        private static bool ForAutoRegistration(Type type)
        {
            return type.GetCustomAttribute<ManualRegistrationAttribute>() == null;
        }
    }
}