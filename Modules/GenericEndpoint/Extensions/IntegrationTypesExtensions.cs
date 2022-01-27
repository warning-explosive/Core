namespace SpaceEngineers.Core.GenericEndpoint.Extensions
{
    using System;
    using System.Linq;
    using Api.Abstractions;
    using Basics;
    using CompositionRoot.Api.Abstractions.Registration;
    using CompositionRoot.Api.Extensions;
    using Contract.Abstractions;

    internal static class IntegrationTypesExtensions
    {
        internal static bool IsMessageContractAbstraction(this Type type)
        {
            return type == typeof(IIntegrationMessage)
                   || type == typeof(IIntegrationCommand)
                   || type == typeof(IIntegrationEvent)
                   || type == typeof(IIntegrationReply)
                   || typeof(IIntegrationQuery<>) == type.GenericTypeDefinitionOrSelf();
        }

        internal static bool HasMessageHandler(this Type messageType, IRegistrationsContainer registrations)
        {
            return registrations
                .Collections()
                .RegisteredComponents()
                .SelectMany(info => info.ExtractGenericArgumentsAt(typeof(IMessageHandler<>)))
                .Any(messageType.IsAssignableFrom);
        }
    }
}