namespace SpaceEngineers.Core.CompositionRoot.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoRegistration.Api.Attributes;
    using Basics;
    using Registration;

    internal static class RegistrationInfoExtensions
    {
        internal static ConstructorInfo GetAutoWiringConstructor(this IConstructorResolutionBehavior constructorResolutionBehavior, Type type)
        {
            if (constructorResolutionBehavior.TryGetConstructor(type, out var cctor))
            {
                return cctor;
            }

            throw new InvalidOperationException($"Component {type} should have constructor suitable for specified constructor resolution behavior in order to be wired in the dependency container");
        }

        internal static IEnumerable<DecoratorRegistrationInfo> GetDecoratorInfo(
            this IEnumerable<Type> decorators,
            Type decoratorType)
        {
            return decorators
                .SelectMany(decorator => decorator
                    .ExtractGenericArgumentsAt(decoratorType)
                    .Select(service => (service, decorator))
                    .Where(info => info.ForAutoRegistration())
                    .Select(AutoRegisteredService())
                    .Select(info => new DecoratorRegistrationInfo(info.Service, info.Implementation, info.Lifestyle)));
        }

        internal static IEnumerable<ServiceRegistrationInfo> GetComponents(
            this IEnumerable<Type> serviceTypes,
            ITypeProvider typeProvider,
            IConstructorResolutionBehavior constructorResolutionBehavior)
        {
            return serviceTypes
               .SelectMany(serviceType => GetTypesToRegister(serviceType, typeProvider, constructorResolutionBehavior)
                   .Select(implementationType => (serviceType, implementationType)))
               .GetComponents()
               .Distinct();
        }

        private static IEnumerable<Type> GetTypesToRegister(
            Type serviceType,
            ITypeProvider typeProvider,
            IConstructorResolutionBehavior constructorResolutionBehavior)
        {
            return AssembliesExtensions
               .AllOurAssembliesFromCurrentDomain()
               .SelectMany(assembly => assembly.GetTypes())
               .Where(type => !serviceType.IsPartiallyClosed()
                           && type.IsConcreteType()
                           && (serviceType.IsGenericType
                                  ? type.IsSubclassOfOpenGeneric(serviceType.GetGenericTypeDefinition())
                                  : serviceType.IsAssignableFrom(type)))
               .Where(type => !constructorResolutionBehavior.TryGetConstructor(type, out var cctor)
                           || !cctor.IsDecorator(serviceType))
               .Where(typeProvider.IsOurType);
        }

        private static IEnumerable<ServiceRegistrationInfo> GetComponents(
            this IEnumerable<(Type Service, Type Implementation)> infos)
        {
            return infos
               .SelectMany(info => GetClosedGenericImplForOpenGenericService(info.Service, info.Implementation)
                   .Select(innerInfo => (innerInfo.Service, innerInfo.Implementation)))
               .Where(info => info.ForAutoRegistration())
               .Select(AutoRegisteredService());
        }

        private static IEnumerable<(Type Service, Type Implementation)> GetClosedGenericImplForOpenGenericService(
            Type serviceType,
            Type implementationType)
        {
            if (serviceType.IsGenericType
             && serviceType.IsGenericTypeDefinition
             && !implementationType.IsGenericType)
            {
                var length = serviceType.GetGenericArguments().Length;

                var argsColumnStore = new Dictionary<int, ICollection<Type>>(length);

                for (var i = 0; i < length; ++i)
                {
                    argsColumnStore[i] = implementationType.ExtractGenericArgumentsAt(serviceType, i).ToList();
                }

                foreach (var typeArguments in argsColumnStore.Values.ColumnsCartesianProduct())
                {
                    var closedService = serviceType.MakeGenericType(typeArguments.ToArray());
                    yield return (closedService, implementationType);
                }
            }
            else
            {
                yield return (serviceType, implementationType);
            }
        }

        private static bool ForAutoRegistration(this (Type service, Type implementation) info)
        {
            var (service, implementation) = info;

            return implementation.IsConcreteType()
                   && !service.HasAttribute<UnregisteredComponentAttribute>()
                   && !implementation.HasAttribute<UnregisteredComponentAttribute>()
                   && !implementation.HasAttribute<ManuallyRegisteredComponentAttribute>()
                   && !implementation.HasAttribute<ComponentOverrideAttribute>();
        }

        private static Func<(Type Service, Type Implementation), ServiceRegistrationInfo> AutoRegisteredService()
        {
            return info =>
            {
                var (service, implementation) = info;

                var lifestyle = implementation.GetRequiredAttribute<ComponentAttribute>().Lifestyle;

                return new ServiceRegistrationInfo(service, implementation, lifestyle);
            };
        }
    }
}