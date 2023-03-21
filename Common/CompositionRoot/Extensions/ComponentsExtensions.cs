namespace SpaceEngineers.Core.CompositionRoot.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoRegistration.Api.Abstractions;
    using Basics;
    using Registration;

    /// <summary>
    /// ComponentsExtensions
    /// </summary>
    public static class ComponentsExtensions
    {
        /// <summary>
        /// Flattens decorators and implementation objects from the source object tree
        /// </summary>
        /// <param name="service">Service implementation</param>
        /// <param name="selector">Result selector</param>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <typeparam name="TResult">TResult type-argument</typeparam>
        /// <returns>Unwrapped decorators and implementation</returns>
        public static IEnumerable<TResult> FlattenDecoratedObject<TService, TResult>(
            this TService service,
            Func<object, TResult> selector)
            where TService : class
        {
            while (service is IDecorator<TService> decorator)
            {
                yield return selector(service);
                service = decorator.Decoratee;
            }

            yield return selector(service);
        }

        /// <summary>
        /// Gets registered components
        /// </summary>
        /// <param name="registrations">IRegistrationsContainer</param>
        /// <returns>Registered components</returns>
        public static IEnumerable<Type> RegisteredComponents(this IRegistrationsContainer registrations)
        {
            return registrations.Instances()
               .Cast<IRegistrationInfo>()
               .Concat(registrations.Resolvable())
               .Concat(registrations.Delegates())
               .Concat(registrations.Collections())
               .Concat(registrations.Decorators())
               .RegisteredComponents();
        }

        /// <summary>
        /// Gets registered components
        /// </summary>
        /// <param name="registrations">Registrations</param>
        /// <returns>Registered components</returns>
        public static IEnumerable<Type> RegisteredComponents(this IEnumerable<IRegistrationInfo> registrations)
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
                    DelegateRegistrationInfo delegateRegistrationInfo => delegateRegistrationInfo.InstanceProducer().GetType(),
                    DecoratorRegistrationInfo decoratorRegistrationInfo => decoratorRegistrationInfo.Implementation,
                    _ => throw new NotSupportedException(info.GetType().Name)
                };
            }
        }
    }
}