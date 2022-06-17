namespace SpaceEngineers.Core.GenericEndpoint.Extensions
{
    using System;
    using System.Linq;
    using Api.Abstractions;
    using Basics;
    using CompositionRoot.Api.Abstractions.Registration;
    using CompositionRoot.Api.Extensions;

    internal static class IntegrationTypesExtensions
    {
        internal static bool HasMessageHandler(this Type messageType, IRegistrationsContainer registrations)
        {
            return registrations
               .Resolvable()
               .RegisteredComponents()
               .SelectMany(info => info.ExtractGenericArgumentsAt(typeof(IMessageHandler<>)))
               .Any(messageType.IsAssignableFrom);
        }
    }
}