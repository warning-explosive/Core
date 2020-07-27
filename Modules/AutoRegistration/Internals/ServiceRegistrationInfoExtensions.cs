namespace SpaceEngineers.Core.AutoRegistration.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using Basics;
    using SimpleInjector;

    internal static class ServiceRegistrationInfoExtensions
    {
        internal static IEnumerable<Type> GetTypesToRegister(this Type serviceType, Container container, ITypeExtensions typeExtensions)
        {
            var versionType = typeof(IVersionFor<>).MakeGenericType(serviceType);

            return container.GetTypesToRegister(serviceType,
                                                typeExtensions.OurAssemblies(),
                                                new TypesToRegisterOptions
                                                {
                                                    IncludeComposites = false,
                                                    IncludeDecorators = false,
                                                    IncludeGenericTypeDefinitions = true
                                                })
                            .Where(impl => !versionType.IsAssignableFrom(impl));
        }

        internal static IEnumerable<ServiceRegistrationInfo> VersionComponents(this IEnumerable<Type> servicesWithVersions, Container container)
        {
            foreach (var service in servicesWithVersions)
            {
                var serviceType = typeof(IVersionedService<>).MakeGenericType(service);
                var componentType = typeof(VersionedService<>).MakeGenericType(service);
                var lifestyle = container.GetRegistration(service).Lifestyle.MapLifestyle();
                yield return new ServiceRegistrationInfo(serviceType, componentType, lifestyle);
            }
        }

        internal static IEnumerable<ServiceRegistrationInfo> GetComponents(this IEnumerable<Type> services, Container container, ITypeExtensions typeExtensions)
        {
            return services.SelectMany(i => i.GetTypesToRegister(container, typeExtensions).GetComponents(i));
        }

        internal static IEnumerable<ServiceRegistrationInfo> GetImplementationComponents(this IEnumerable<Type> services)
        {
            return services.GetComponents(typeof(IResolvableImplementation));
        }

        internal static IEnumerable<ServiceRegistrationInfo> GetDecoratorInfo(this IEnumerable<Type> decorators, Type decoratorType)
        {
            return GetGenericDecoratorInfo(decorators, decoratorType).Select(pair => pair.Info);
        }

        internal static IEnumerable<ServiceRegistrationInfo> GetConditionalDecoratorInfo(this IEnumerable<Type> decorators, Type decoratorType)
        {
            return GetGenericDecoratorInfo(decorators, decoratorType)
               .Select(pair =>
                       {
                           pair.Info.Attribute = pair.Decorator.GetGenericArguments()[1];
                           return pair.Info;
                       });
        }

        private static IEnumerable<(Type Decorator, ServiceRegistrationInfo Info)> GetGenericDecoratorInfo(IEnumerable<Type> decorators, Type decoratorType)
        {
            return decorators.Where(ForAutoRegistration)
                             .Select(t => new
                                          {
                                              ComponentType = t,
                                              Decorator = ExtractDecorator(decoratorType, t),
                                              t.GetCustomAttribute<LifestyleAttribute>()?.Lifestyle
                                          })
                             .Select(t => (t.Decorator, new ServiceRegistrationInfo(t.Decorator.GetGenericArguments()[0], t.ComponentType, t.Lifestyle)));
        }

        private static Type ExtractDecorator(Type decoratorType, Type t)
        {
            return t.GetInterfaces()
                    .Where(i => i.IsGenericType)
                    .Single(i => i.GetGenericTypeDefinition() == decoratorType);
        }

        private static IEnumerable<ServiceRegistrationInfo> GetComponents(this IEnumerable<Type> types, Type serviceType)
        {
            return types
                  .Where(ForAutoRegistration)
                  .SelectMany(cmp => GetClosedGenericImplForOpenGenericService(serviceType, cmp))
                  .Select(pair => new ServiceRegistrationInfo(pair.ServiceType, pair.ComponentType, pair.ComponentType.GetCustomAttribute<LifestyleAttribute>()?.Lifestyle));
        }

        private static bool ForAutoRegistration(Type type)
        {
            return type.GetCustomAttribute<UnregisteredAttribute>(true) == null
                   && type.GetCustomAttribute<ManualRegistrationAttribute>(true) == null;
        }

        private static IEnumerable<(Type ComponentType, Type ServiceType)> GetClosedGenericImplForOpenGenericService(Type serviceType, Type componentType)
        {
            if (serviceType.IsGenericType
             && serviceType.IsGenericTypeDefinition
             && !componentType.IsGenericType)
            {
                var length = serviceType.GetGenericArguments().Length;

                var argsColumnStore = new Dictionary<int, ICollection<Type>>(length);

                for (var i = 0; i < length; ++i)
                {
                    argsColumnStore[i] = componentType.GetGenericArgumentsOfOpenGenericAt(serviceType, i).ToList();
                }

                foreach (var typeArguments in argsColumnStore.Values.ColumnsCartesianProduct())
                {
                    var closedService = serviceType.MakeGenericType(typeArguments.ToArray());
                    yield return (componentType, closedService);
                }
            }
            else
            {
                yield return (componentType, serviceType);
            }
        }
    }
}