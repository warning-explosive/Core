namespace SpaceEngineers.Core.GenericEndpoint.Endpoint
{
    using System;
    using System.Linq;
    using Api.Abstractions;
    using Basics;
    using CompositionRoot.Extensions;
    using CompositionRoot.Registration;

    internal static class IntegrationTypesExtensions
    {
        internal static bool HasMessageHandler(this Type messageType, IRegistrationsContainer registrations)
        {
            return registrations
               .Resolvable()
               .RegisteredComponents()
               .Where(type => type.IsSubclassOfOpenGeneric(typeof(IMessageHandler<>)))
               .SelectMany(info => info.ExtractGenericArgumentsAt(typeof(IMessageHandler<>)))
               .Any(arg => messageType.IsAssignableFrom(arg.GenericTypeDefinitionOrSelf()));
        }
    }
}