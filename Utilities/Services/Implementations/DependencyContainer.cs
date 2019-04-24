namespace SpaceEngineers.Core.Utilities.Services.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Attributes;
    using Enumerations;
    using Extensions;
    using Interfaces;
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
                throw new Exception($"ServiceType must be an interface and derived from {nameof(IResolvable)}");
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
                                    DefaultScopedLifestyle = ScopedLifestyle.Flowing,
                                }
                            };

            RegisterServices(container);

            container.Verify(VerificationOption.VerifyAndDiagnose);

            return container;
        }
        
        private static IEnumerable<ServiceRegistrationInfo> GetValidServiceComponentPairs<T>(Container container)
        {
            var components = AppDomain.CurrentDomain.GetAssemblies()
                                      .AsParallel()
                                      .SelectMany(assembly => assembly.GetTypes())
                                      .Where(type => !type.IsInterface
                                                     && !type.IsAbstract
                                                     && type.IsDerivedFromInterface(typeof(T)))
                                      .Select(componentType => new
                                                               {
                                                                   ComponentType = componentType,
                                                                   ServiceTypes = componentType.GetInterfaces()
                                                                                               .Where(i => typeof(T).IsContainsInterfaceDeclaration(i))
                                                                                               .ToArray()
                                                               })
                                      .ToArray();

            var ambigous = components.Where(pair => pair.ServiceTypes.Length > 1)
                                     .ToArray();

            if (ambigous.Any())
            {
                var errors = ambigous.Select(pairs => $"Component - {pairs.ComponentType.Name} - contains ambigous definition of IResolvable services - {string.Join(", ", pairs.ServiceTypes.Select(i => i.Name))}");
                throw new AmbiguousMatchException(string.Join("\n", errors));
            }
            
            var typePairs = components
                           .Select(pair => new
                                   {
                                       pair.ComponentType,
                                       ServiceType = pair.ServiceTypes.Single(),
                                       pair.ComponentType.GetCustomAttribute<LifestyleAttribute>()?.Lifestyle
                                   })
                           .ToArray();

            var lifestyleErrors = typePairs.Where(pair => !pair.Lifestyle.HasValue)
                                           .ToArray();
            
            if (lifestyleErrors.Any())
            {
                var errors = lifestyleErrors.Select(pair => new Exception($"Component - {pair.ComponentType.Name} - must have {nameof(LifestyleAttribute)}"));
                throw new Exception(string.Join("\n", errors));
            }
            
            return typePairs.Select(pair => new ServiceRegistrationInfo(pair.ServiceType,
                                                                        pair.ComponentType,
                                                                        pair.Lifestyle.Value));
        }
        
        private static void RegisterServices(Container container)
        {
            foreach (var iResolvable in GetValidServiceComponentPairs<IResolvable>(container))
            {
                var lifestyle = GetLifestyle(iResolvable.EnLifestyle);
                
                if (iResolvable.ServiceType.IsGenericType)
                {
                    container.Register(iResolvable.ServiceType.GetGenericTypeDefinition(), iResolvable.ComponentType, lifestyle);
                }
                else
                {
                    container.Register(iResolvable.ServiceType, iResolvable.ComponentType, lifestyle);
                }
            }
        }

        private static Lifestyle GetLifestyle(EnLifestyle enLifestyle)
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