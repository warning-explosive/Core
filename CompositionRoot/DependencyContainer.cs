namespace SpaceEngineers.Core.CompositionRoot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abstractions;
    using Attributes;
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
        /// <param name="assemblies">assemblies</param>
        public DependencyContainer(Assembly[] assemblies) { _container = InitContainer(assemblies); }
        
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
            if (!serviceType.IsDerivedFromInterface(typeof(IResolvable)))
            {
                throw new ArgumentException($"{nameof(serviceType)} must be an interface and derived from {nameof(IResolvable)}");
            }

            return _container.GetInstance(serviceType);
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
            if (!serviceType.IsDerivedFromInterface(typeof(ICollectionResolvable)))
            {
                throw new ArgumentException($"{nameof(serviceType)} must be an interface and derived from {nameof(ICollectionResolvable)}");
            }
            
            return _container.GetAllInstances(serviceType);
        }
        
        private static Container InitContainer(Assembly[] assemblies)
        {
            var typeExtensions = TypeExtensions.SetInstance(assemblies);
            
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
                                }
                            };
            
            RegisterServices(container, typeExtensions);

            container.Verify(VerificationOption.VerifyAndDiagnose);

            return container;
        }

        private static void RegisterServices(Container container, ITypeExtensions typeExtensions)
        {
            /*
             * [I] - Single
             */
            var iResolvableRegistrationInfos = GetServiceRegistrationInfo<IResolvable>(container, typeExtensions);
           
            foreach (var iResolvable in iResolvableRegistrationInfos)
            {
                container.Register(iResolvable.ServiceType, iResolvable.ComponentType, iResolvable.Lifestyle);
            }
            
            /*
             * [II] - Collections
             */
            var iCollectionResolvableRegistrationInfos = GetServiceRegistrationInfo<ICollectionResolvable>(container, typeExtensions);
            RegisterLifestyle(container, iCollectionResolvableRegistrationInfos);

            foreach (var iCollectionResolvable in iCollectionResolvableRegistrationInfos
                                                 .OrderBy(info => OrderByAttribute(info, typeExtensions))
                                                 .GroupBy(k => k.ServiceType, v => v.ComponentType))
            {
                container.Collection.Register(iCollectionResolvable.Key, iCollectionResolvable);
            }

            /*
             * [III] - Decorators
             */
            GetDecoratorInfo(typeExtensions, typeof(IDecorator<>))
               .Concat(GetConditionalDecoratorInfo(typeExtensions, typeof(IConditionalDecorator<,>)))
               .OrderBy(info => OrderByAttribute(info, typeExtensions))
               .Each(info => RegisterDecorator(container, info));
            
            /*
             * [IV] - Collection decorators
             */
            GetDecoratorInfo(typeExtensions, typeof(ICollectionDecorator<>))
               .Concat(GetConditionalDecoratorInfo(typeExtensions, typeof(ICollectionConditionalDecorator<,>)))
               .OrderBy(info => OrderByAttribute(info, typeExtensions))
               .Each(info => RegisterDecorator(container, info));
        }

        private static ServiceRegistrationInfo[] GetServiceRegistrationInfo<TInterface>(Container container,
                                                                                        ITypeExtensions typeExtensions)
        {
            return typeExtensions
                  .AllOurServicesThatContainsDeclarationOfInterface<TInterface>()
                  .Select(i => new
                               {
                                   ServiceType = i,
                                   Components = container
                                               .GetTypesToRegister(i,
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

        private static void RegisterLifestyle(Container container,
                                              IEnumerable<ServiceRegistrationInfo> serviceRegistrationInfos)
        {
            foreach (var info in serviceRegistrationInfos)
            {
                container.Register(info.ComponentType,
                                   info.ComponentType,
                                   info.Lifestyle);
            }
        }

        private static IEnumerable<ServiceRegistrationInfo> GetDecoratorInfo(ITypeExtensions typeExtensions,
                                                                             Type decoratorType)
        {
            return typeExtensions
                  .OurTypes()
                  .Where(cmp => typeExtensions.IsImplementationOfOpenGenericInterface(cmp, decoratorType))
                  .Select(cmp => new ServiceRegistrationInfo(cmp.GetInterfaces()
                                                                .Where(i => i.IsGenericType)
                                                                .Single(i => i.GetGenericTypeDefinition() == decoratorType)
                                                                .GetGenericArguments()[0],
                                                             cmp,
                                                             cmp.GetCustomAttribute<LifestyleAttribute>()?.Lifestyle));
        }

        private static IEnumerable<ServiceRegistrationInfo> GetConditionalDecoratorInfo(ITypeExtensions typeExtensions,
                                                                                        Type decoratorType)
        {
            return typeExtensions
                  .OurTypes()
                  .Where(t => typeExtensions.IsImplementationOfOpenGenericInterface(t,
                                                                                    decoratorType))
                  .Select(t => new
                               {
                                   ComponentType = t,
                                   Decorator = t.GetInterfaces()
                                                .Where(i => i.IsGenericType)
                                                .Single(i => i.GetGenericTypeDefinition() == decoratorType)
                               })
                  .Select(t => new ServiceRegistrationInfo(t.Decorator.GetGenericArguments()[0],
                                                           t.ComponentType,
                                                           t.ComponentType.GetCustomAttribute<LifestyleAttribute>()
                                                           ?.Lifestyle)
                               {
                                   Attribute = t.Decorator.GetGenericArguments()[1]
                               });
        }

        private static uint OrderByAttribute(ServiceRegistrationInfo info, ITypeExtensions typeExtensions)
        {
            return typeExtensions.GetOrder(info.ComponentType) ?? uint.MaxValue;
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