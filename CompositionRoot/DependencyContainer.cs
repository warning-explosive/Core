namespace SpaceEngineers.Core.CompositionRoot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abstractions;
    using Attributes;
    using Exceptions;
    using Extensions;
    using SimpleInjector;

    /// <summary>
    /// Dependency container
    /// Resolve dependencies by 'Dependency Injection' and 'Inversion Of Control' patterns
    /// </summary>
    public static class DependencyContainer
    {
        private static readonly Container _container = InitContainer();
        
        /// <summary>
        /// Resolve service implementation
        /// </summary>
        /// <typeparam name="T">IResolvable</typeparam>
        /// <returns>Service implementation</returns>
        public static T Resolve<T>()
            where T : class, IResolvable
        {
            return _container.GetInstance<T>();
        }

        /// <summary>
        /// Resolve untyped service implementation
        /// </summary>
        /// <param name="serviceType">IResolvable</param>
        /// <returns>Untyped service implementation</returns>
        public static object Resolve(Type serviceType)
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
        public static IEnumerable<T> ResolveCollection<T>()
            where T : class, ICollectionResolvable
        {
            return _container.GetAllInstances<T>();
        }

        /// <summary>
        /// Resolve untyped service implementations collection
        /// </summary>
        /// <param name="serviceType">IResolvable</param>
        /// <returns>Untyped service implementation</returns>
        public static IEnumerable<object> ResolveCollection(Type serviceType)
        {
            if (!serviceType.IsDerivedFromInterface(typeof(ICollectionResolvable)))
            {
                throw new ArgumentException($"{nameof(serviceType)} must be an interface and derived from {nameof(ICollectionResolvable)}");
            }
            
            return _container.GetAllInstances(serviceType);
        }
        
        private static Container InitContainer()
        {
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

            var typeInfoStorage = new TypeInfoStorage();
            var typeExtensions = new TypeExtensionsImpl(typeInfoStorage);
            
            container.RegisterInstance<ITypeInfoStorage>(typeInfoStorage);
            container.RegisterInstance<ITypeExtensions>(typeExtensions);

            var registredByHand = new[] { typeof(TypeInfoStorage), typeof(TypeExtensionsImpl) };
            
            RegisterServices(container, typeInfoStorage, typeExtensions, registredByHand);

            container.Verify(VerificationOption.VerifyAndDiagnose);

            return container;
        }

        private static void RegisterServices(Container container,
                                             ITypeInfoStorage typeInfoStorage,
                                             ITypeExtensions typeExtensions,
                                             Type[] registredByHand)
        {
            var iResolvableRegistrationInfos = GetServiceRegistrationInfo<IResolvable>(container,
                                                                                       typeInfoStorage,
                                                                                       typeExtensions,
                                                                                       registredByHand);
           
            foreach (var iResolvable in iResolvableRegistrationInfos)
            {
                container.Register(iResolvable.ServiceType, iResolvable.ComponentType, iResolvable.Lifestyle);
            }

            var iCollectionResolvableRegistrationInfos = GetServiceRegistrationInfo<ICollectionResolvable>(container,
                                                                                                           typeInfoStorage,
                                                                                                           typeExtensions,
                                                                                                           registredByHand);

            RegisterLifestyle(container, iCollectionResolvableRegistrationInfos);

            foreach (var iCollectionResolvable in iCollectionResolvableRegistrationInfos
                                                 .OrderBy(cmp => typeInfoStorage[cmp.ComponentType].Order ?? uint.MaxValue)
                                                 .ToDictionary(k => k.ServiceType, v => v.ComponentType))
            {
                container.Collection.Register(iCollectionResolvable.Key, iCollectionResolvable.Value);
            }
        }

        private static ServiceRegistrationInfo[] GetServiceRegistrationInfo<TInterface>(
            Container container,
            ITypeInfoStorage typeInfoStorage,
            ITypeExtensions typeExtensions,
            Type[] registredByHand)
        {
            return typeInfoStorage
                  .OurTypes
                  .Where(t => t.IsInterface
                              && typeExtensions.IsContainsInterfaceDeclaration(t, typeof(TInterface)))
                  .Select(i => new
                               {
                                   ServiceType = i,
                                   Components = container
                                               .GetTypesToRegister(i,
                                                                   typeInfoStorage.OurAssemblies,
                                                                   new TypesToRegisterOptions
                                                                   {
                                                                       IncludeComposites = true,
                                                                       IncludeDecorators = true,
                                                                       IncludeGenericTypeDefinitions = true 
                                                                   })
                                               .Except(registredByHand)
                                               .Select(cmp => new
                                                              {
                                                                  Component = cmp,
                                                                  cmp.GetCustomAttribute<LifestyleAttribute>()?.Lifestyle
                                                              })
                               })
                  .SelectMany(info => info.Components
                                          .Select(cmp =>
                                                  {
                                                      if (cmp.Lifestyle == null)
                                                      {
                                                          throw new AttributeRequiredException(typeof(LifestyleAttribute), cmp.Component);
                                                      }

                                                      return new ServiceRegistrationInfo(info.ServiceType,
                                                                                         cmp.Component,
                                                                                         cmp.Lifestyle.Value);
                                                  }))
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
    }
}