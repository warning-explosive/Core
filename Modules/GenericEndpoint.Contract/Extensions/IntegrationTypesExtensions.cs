namespace SpaceEngineers.Core.GenericEndpoint.Contract.Extensions
{
    using System;
    using Abstractions;
    using Attributes;
    using Basics;
    using Contract;

    /// <summary>
    /// IntegrationTypesExtensions
    /// </summary>
    public static class IntegrationTypesExtensions
    {
        /// <summary>
        /// Is message type representing contracts abstraction
        /// </summary>
        /// <param name="type">Message type</param>
        /// <returns>Message type is representing contracts abstraction</returns>
        public static bool IsMessageContractAbstraction(this Type type)
        {
            return type == typeof(IIntegrationMessage)
                   || type == typeof(IIntegrationCommand)
                   || type == typeof(IIntegrationEvent)
                   || type == typeof(IIntegrationReply)
                   || typeof(IIntegrationQuery<>) == type.GenericTypeDefinitionOrSelf();
        }

        /// <summary>
        /// Is message type owned by specified endpoint
        /// </summary>
        /// <param name="type">Message type</param>
        /// <param name="endpointIdentity">EndpointIdentity</param>
        /// <returns>Message type is owned by specified endpoint</returns>
        public static bool OwnedByEndpoint(this Type type, EndpointIdentity endpointIdentity)
        {
            return type
               .GetRequiredAttribute<OwnedByAttribute>()
               .EndpointName
               .Equals(endpointIdentity.LogicalName, StringComparison.OrdinalIgnoreCase);
        }
    }
}