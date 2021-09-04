namespace SpaceEngineers.Core.CompositionRoot.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Api.Abstractions.Registration;
    using Basics;

    internal static class RegistrationsContainerExtensions
    {
        internal static IEnumerable<Type> RegisteredComponents(this IRegistrationsContainer registrations)
        {
            return registrations.Instances().Select(singleton => singleton.Instance.GetType())
                .Concat(registrations.Resolvable().Select(info => info.Implementation))
                .Concat(registrations.Delegates().Select(info => info.Service))
                .Concat(registrations.Collections().Select(info => info.Implementation))
                .Concat(registrations.Decorators().Select(info => info.Implementation))
                .Where(type => type.IsConcreteType());
        }
    }
}