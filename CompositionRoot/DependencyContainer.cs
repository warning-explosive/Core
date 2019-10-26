namespace SpaceEngineers.Core.CompositionRoot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abstractions;
    using Attributes;
    using Enumerations;
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
            var typeInfoStorage = Resolve<ITypeInfoStorage>();
            
            return _container.GetAllInstances<T>()
                             .OrderBy(cmp => typeInfoStorage[cmp.GetType()].Order ?? uint.MaxValue);
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
            
            var typeInfoStorage = Resolve<ITypeInfoStorage>();

            return _container.GetAllInstances(serviceType)
                             .OrderBy(cmp => typeInfoStorage[cmp.GetType()].Order ?? uint.MaxValue);
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
            
            RegisterServices(container, typeInfoStorage, typeExtensions);

            container.Verify(VerificationOption.VerifyAndDiagnose);

            return container;
        }

        private static void RegisterServices(Container container,
                                             ITypeInfoStorage typeInfoStorage,
                                             ITypeExtensions typeExtensions)
        {
            foreach (var iResolvable in GetValidServiceComponentPairs<IResolvable>(typeInfoStorage, typeExtensions))
            {
                container.Register(iResolvable.ServiceType,
                                   iResolvable.ComponentType,
                                   iResolvable.Lifestyle);
            }
            
            foreach (var iCollectionResolvable in GetValidServiceComponentPairs<ICollectionResolvable>(typeInfoStorage, typeExtensions))
            {
                container.Collection.Append(iCollectionResolvable.ServiceType,
                                            iCollectionResolvable.ComponentType,
                                            iCollectionResolvable.Lifestyle);
            }
        }

        private static IEnumerable<ServiceRegistrationInfo> GetValidServiceComponentPairs<TInterface>(
            ITypeInfoStorage typeInfoStorage,
            ITypeExtensions typeExtensions)
        {
            return typeInfoStorage
                  .OurTypes
                  .Except(new[] { typeof(TypeInfoStorage), typeof(TypeExtensionsImpl) })
                  .Where(type => !type.IsInterface
                                 && !type.IsAbstract
                                 && typeExtensions.IsDerivedFromInterface(type, typeof(TInterface)))
                  .Select(componentType => new
                                           {
                                               ComponentType = componentType,
                                               ServiceTypes = componentType.GetInterfaces()
                                                                           .Where(i => typeExtensions.IsContainsInterfaceDeclaration(i, typeof(TInterface)))
                                                                           .ToArray(),
                                               componentType.GetCustomAttribute<LifestyleAttribute>()?.Lifestyle
                                           })
                  .Select(pair =>
                          {
                              if (pair.ServiceTypes.Length > 1)
                              {
                                  throw new AmbiguousMatchException($"Component '{pair.ComponentType.Name}' contains ambigous definition of IResolvable services '{string.Join(", ", pair.ServiceTypes.Select(i => i.Name))}'");
                              }
                              
                              if (!pair.Lifestyle.HasValue)
                              {
                                  throw new AttributeRequiredException(typeof(LifestyleAttribute), pair.ComponentType);
                              }

                              return new ServiceRegistrationInfo(pair.ServiceTypes.Single(),
                                                                 pair.ComponentType,
                                                                 pair.Lifestyle.Value);
                          });
        }
    }
}