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
    /// Resolve dependencies by 'Dependency Injection' and 'Inversion Of Control' patterns
    /// </summary>
    public class DependencyContainer
    {
        private readonly Container _container;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="assemblies">All loaded assemblies</param>
        /// <param name="rootAssemblies">Root assemblies</param>
        public DependencyContainer(Assembly[] assemblies, Assembly[] rootAssemblies)
        {
            _container = InitContainer(assemblies, rootAssemblies);
        }

        /// <summary>
        /// Default configured container
        /// Assemblies loaded in CurrentDomain and SpaceEngineers.Core.AutoWiringApi assembly as root assembly
        /// </summary>
        /// <returns>DependencyContainer</returns>
        public static DependencyContainer Default()
        {
            return new DependencyContainer(AssembliesExtensions.AllFromCurrentDomain(),
                                           new[] { typeof(LifestyleAttribute).Assembly });
        }

        /// <summary>
        /// Resolve service implementation
        /// </summary>
        /// <typeparam name="T">IResolvable</typeparam>
        /// <returns>Service implementation</returns>
        public T Resolve<T>()
            where T : class, IResolvable
        {
            return _container.GetInstance<T>();
        }

        /// <summary>
        /// Resolve untyped service implementation
        /// </summary>
        /// <param name="serviceType">IResolvable</param>
        /// <returns>Untyped service implementation</returns>
        public object Resolve(Type serviceType)
        {
            if (!typeof(IResolvable).IsAssignableFrom(serviceType))
            {
                throw new ArgumentException($"{serviceType.FullName} must be an interface and derived from {nameof(IResolvable)}");
            }

            return _container.GetInstance(serviceType);
        }

        /// <summary>
        /// Resolve concrete implementation
        /// </summary>
        /// <typeparam name="T">IResolvable</typeparam>
        /// <returns>Service implementation</returns>
        public T ResolveImplementation<T>()
            where T : class, IResolvableImplementation
        {
            return _container.GetInstance<T>();
        }

        /// <summary>
        /// Resolve untyped concrete implementation
        /// </summary>
        /// <param name="concreteImplementationType">IResolvableImplementation</param>
        /// <returns>Untyped concrete implementation</returns>
        public object ResolveImplementation(Type concreteImplementationType)
        {
            if (!typeof(IResolvableImplementation).IsAssignableFrom(concreteImplementationType))
            {
                throw new ArgumentException($"{concreteImplementationType.FullName} must be an concrete type and derived from {nameof(IResolvableImplementation)}");
            }

            return _container.GetInstance(concreteImplementationType);
        }

        /// <summary>
        /// Resolve service implementations collection
        /// </summary>
        /// <typeparam name="T">IResolvable</typeparam>
        /// <returns>Service implementation</returns>
        public IEnumerable<T> ResolveCollection<T>()
            where T : class, ICollectionResolvable
        {
            return _container.GetAllInstances<T>();
        }

        /// <summary>
        /// Resolve untyped service implementations collection
        /// </summary>
        /// <param name="serviceType">IResolvable</param>
        /// <returns>Untyped service implementation</returns>
        public IEnumerable<object> ResolveCollection(Type serviceType)
        {
            if (!typeof(ICollectionResolvable).IsAssignableFrom(serviceType))
            {
                throw new ArgumentException($"{serviceType.FullName} must be an interface and derived from {nameof(ICollectionResolvable)}");
            }

            return _container.GetAllInstances(serviceType);
        }

        private static Container InitContainer(Assembly[] assemblies, Assembly[] rootAssemblies)
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

            RegisterServices(container, typeExtensions);

            container.Verify(VerificationOption.VerifyAndDiagnose);

            return container;
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
             * [II] - Collection resolvable service
             */
            var collectionResolvableRegistrationInfos = GetServiceRegistrationInfo<ICollectionResolvable>(container, typeExtensions);

            // register each element of collection as implementation to provide lifestyle to container
            RegisterImplementations(container, collectionResolvableRegistrationInfos);

            typeExtensions.OrderByDependencies(collectionResolvableRegistrationInfos, z => z.ComponentType)
                          .GroupBy(k => k.ServiceType, v => v.ComponentType)
                          .Each(info => container.Collection.Register(info.Key, info));

            /*
             * [III] - Decorators
             */
            var decoratorInfos = GetDecoratorInfo(typeExtensions, typeof(IDecorator<>))
               .Concat(GetConditionalDecoratorInfo(typeExtensions, typeof(IConditionalDecorator<,>)));

            typeExtensions.OrderByDependencies(decoratorInfos, z => z.ComponentType)
                          .Each(info => RegisterDecorator(container, info));

            /*
             * [IV] - Collection decorators
             */
            var collectionDecoratorInfos = GetDecoratorInfo(typeExtensions, typeof(ICollectionDecorator<>))
               .Concat(GetConditionalDecoratorInfo(typeExtensions, typeof(ICollectionConditionalDecorator<,>)));

            typeExtensions.OrderByDependencies(collectionDecoratorInfos, z => z.ComponentType)
                          .Each(info => RegisterDecorator(container, info));

            /*
             * [V] - Concrete Implementations
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

        private static ServiceRegistrationInfo[] GetServiceRegistrationInfo<TInterface>(Container container,
                                                                                        ITypeExtensions typeExtensions)
        {
            return typeExtensions
                  .AllOurServicesThatContainsDeclarationOfInterface<TInterface>()
                  .Select(i => new
                  {
                      ServiceType = i,
                      Components = container.GetTypesToRegister(i,
                                                                typeExtensions.OurAssemblies(),
                                                                new TypesToRegisterOptions
                                                                {
                                                                    IncludeComposites = false,
                                                                    IncludeDecorators = false,
                                                                    IncludeGenericTypeDefinitions = true
                                                                })
                                            .Select(cmp => new
                                            {
                                                ComponentType = cmp,
                                                cmp.GetCustomAttribute<LifestyleAttribute>()?.Lifestyle
                                            })
                  })
                  .SelectMany(info => info.Components
                                          .Select(cmp => new ServiceRegistrationInfo(info.ServiceType,
                                                                                     cmp.ComponentType,
                                                                                     cmp.Lifestyle)))
                  .ToArray();
        }

        private static ServiceRegistrationInfo[] GetImplementationRegistrationInfo(Container container,
                                                                                   ITypeExtensions typeExtensions)
        {
            var implementationType = typeof(IResolvableImplementation);

            return container.GetTypesToRegister(implementationType,
                                                typeExtensions.OurAssemblies(),
                                                new TypesToRegisterOptions
                                                {
                                                    IncludeComposites = false,
                                                    IncludeDecorators = false,
                                                    IncludeGenericTypeDefinitions = true
                                                })
                            .Select(cmp => new
                                           {
                                               ComponentType = cmp,
                                               cmp.GetCustomAttribute<LifestyleAttribute>()?.Lifestyle
                                           })
                            .Select(cmp => new ServiceRegistrationInfo(cmp.ComponentType,
                                                  cmp.ComponentType,
                                                  cmp.Lifestyle))
                            .ToArray();
        }

        private static IEnumerable<ServiceRegistrationInfo> GetDecoratorInfo(ITypeExtensions typeExtensions,
                                                                             Type decoratorType)
        {
            return typeExtensions
                  .OurTypes()
                  .Where(t => !t.IsInterface
                             && typeExtensions.IsSubclassOfOpenGeneric(t, decoratorType))
                  .Select(t => new
                          {
                              ComponentType = t,
                              Decrator = ExtractDecorator(decoratorType, t),
                              t.GetCustomAttribute<LifestyleAttribute>()?.Lifestyle
                          })
                  .Select(t => new ServiceRegistrationInfo(t.Decrator.GetGenericArguments()[0], t.ComponentType, t.Lifestyle));
        }

        private static IEnumerable<ServiceRegistrationInfo> GetConditionalDecoratorInfo(ITypeExtensions typeExtensions,
                                                                                        Type decoratorType)
        {
            return typeExtensions
                  .OurTypes()
                  .Where(t => !t.IsInterface
                              && typeExtensions.IsSubclassOfOpenGeneric(t, decoratorType))
                  .Select(t => new
                  {
                      ComponentType = t,
                      Decorator = ExtractDecorator(decoratorType, t),
                      t.GetCustomAttribute<LifestyleAttribute>()?.Lifestyle
                  })
                  .Select(t => new ServiceRegistrationInfo(t.Decorator.GetGenericArguments()[0], t.ComponentType, t.Lifestyle)
                  {
                      Attribute = t.Decorator.GetGenericArguments()[1]
                  });
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
    }
}