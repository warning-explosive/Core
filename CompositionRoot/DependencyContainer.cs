namespace SpaceEngineers.Core.CompositionRoot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abstractions;
    using Attributes;
    using Enumerations;
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
                var lifestyle = MapLifestyle(iResolvable.EnLifestyle);

                container.Register(iResolvable.ServiceType.IsGenericType ? iResolvable.ServiceType.GetGenericTypeDefinition() : iResolvable.ServiceType,
                                   iResolvable.ComponentType,
                                   lifestyle);
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
                                  throw new AttributeRequiredException(typeof(LifestyleAttribute));
                              }

                              return new ServiceRegistrationInfo(pair.ServiceTypes.Single(),
                                                                 pair.ComponentType,
                                                                 pair.Lifestyle.Value);
                          });
        }

        private static Lifestyle MapLifestyle(EnLifestyle enLifestyle)
        {
            switch (enLifestyle)
            {
                case EnLifestyle.Transient: return Lifestyle.Transient;
                case EnLifestyle.Singleton: return Lifestyle.Singleton;
                case EnLifestyle.Scoped:    return Lifestyle.Scoped;
                default:                    return Lifestyle.Transient;
            }
        }
    }
}