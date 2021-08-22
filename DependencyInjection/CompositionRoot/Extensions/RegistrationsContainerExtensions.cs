namespace SpaceEngineers.Core.CompositionRoot.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using Basics;

    internal static class RegistrationsContainerExtensions
    {
        internal static IEnumerable<Type> RegisteredComponents(this IRegistrationsContainer registrations)
        {
            return registrations.Singletons().Select(singleton => singleton.Type)
                .Concat(registrations.Resolvable().Select(info => info.Implementation))
                .Concat(registrations.Delegates().Select(info => info.Service))
                .Concat(registrations.Collections().Select(info => info.Implementation))
                .Concat(registrations.Decorators().Select(info => info.Implementation))
                .Where(type => type.IsConcreteType());
        }
    }
}