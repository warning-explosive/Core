namespace SpaceEngineers.Core.CompositionRoot.Api.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions.Registration;
    using Basics;

    internal static class RegistrationsContainerExtensions
    {
        internal static IEnumerable<Type> RegisteredComponents(this IRegistrationsContainer registrations)
        {
            return registrations.Instances()
                .Cast<IRegistrationInfo>()
                .Concat(registrations.Resolvable())
                .Concat(registrations.Delegates())
                .Concat(registrations.Collections())
                .Concat(registrations.Decorators())
                .RegisteredComponents();
        }

        internal static IEnumerable<Type> RegisteredComponents(this IEnumerable<IRegistrationInfo> registrations)
        {
            return registrations
                .Select(RegisteredComponent)
                .Where(type => type.IsConcreteType());

            static Type RegisteredComponent(IRegistrationInfo info)
            {
                return info switch
                {
                    InstanceRegistrationInfo instanceRegistrationInfo => instanceRegistrationInfo.Instance.GetType(),
                    ServiceRegistrationInfo serviceRegistrationInfo => serviceRegistrationInfo.Implementation,
                    DelegateRegistrationInfo delegateRegistrationInfo => delegateRegistrationInfo.Service,
                    DecoratorRegistrationInfo decoratorRegistrationInfo => decoratorRegistrationInfo.Implementation,
                    _ => throw new NotSupportedException(info.GetType().Name)
                };
            }
        }
    }
}