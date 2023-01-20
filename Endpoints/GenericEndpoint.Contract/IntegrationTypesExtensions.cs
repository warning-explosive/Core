namespace SpaceEngineers.Core.GenericEndpoint.Contract
{
    using System;
    using Abstractions;
    using Attributes;
    using Basics;

    /// <summary>
    /// IntegrationTypesExtensions
    /// </summary>
    public static class IntegrationTypesExtensions
    {
        /// <summary>
        /// Does type represent an IIntegrationCommand
        /// </summary>
        /// <param name="type">Message type</param>
        /// <returns>Message type is an IIntegrationCommand or not</returns>
        public static bool IsCommand(this Type type)
        {
            return typeof(IIntegrationCommand).IsAssignableFrom(type);
        }

        /// <summary>
        /// Does type represent an IIntegrationEvent
        /// </summary>
        /// <param name="type">Message type</param>
        /// <returns>Message type is an IIntegrationEvent or not</returns>
        public static bool IsEvent(this Type type)
        {
            return typeof(IIntegrationEvent).IsAssignableFrom(type);
        }

        /// <summary>
        /// Does type represent an IIntegrationRequest
        /// </summary>
        /// <param name="type">Message type</param>
        /// <returns>Message type is an IIntegrationRequest or not</returns>
        public static bool IsRequest(this Type type)
        {
            return type.IsSubclassOfOpenGeneric(typeof(IIntegrationRequest<>));
        }

        /// <summary>
        /// Does type represent an IIntegrationReply
        /// </summary>
        /// <param name="type">Message type</param>
        /// <returns>Message type is an IIntegrationReply or not</returns>
        public static bool IsReply(this Type type)
        {
            return typeof(IIntegrationReply).IsAssignableFrom(type);
        }

        /// <summary>
        /// Does type represent an IIntegrationReply on IIntegrationRequest
        /// </summary>
        /// <param name="reply">Reply message type</param>
        /// <param name="request">Request message type</param>
        /// <returns>Message type is an IIntegrationReply on IIntegrationRequest or not</returns>
        public static bool IsReplyOnRequest(this Type reply, Type request)
        {
            return typeof(IIntegrationReply).IsAssignableFrom(reply)
                && request.IsSubclassOfOpenGeneric(typeof(IIntegrationRequest<>))
                && typeof(IIntegrationRequest<>).MakeGenericType(reply).IsAssignableFrom(request);
        }

        /// <summary>
        /// Does message type represent contracts abstraction
        /// </summary>
        /// <param name="type">Message type</param>
        /// <returns>Message type is representing contracts abstraction</returns>
        public static bool IsMessageContractAbstraction(this Type type)
        {
            return type == typeof(IIntegrationMessage)
                   || type == typeof(IIntegrationCommand)
                   || type == typeof(IIntegrationEvent)
                   || type == typeof(IIntegrationReply)
                   || typeof(IIntegrationRequest<>) == type.GenericTypeDefinitionOrSelf();
        }

        /// <summary>
        /// Is message type owned by specified endpoint
        /// </summary>
        /// <param name="type">Message type</param>
        /// <param name="endpointIdentity">EndpointIdentity</param>
        /// <returns>Message type is owned by specified endpoint</returns>
        public static bool IsOwnedByEndpoint(this Type type, EndpointIdentity endpointIdentity)
        {
            var endpointName = type
               .GetRequiredAttribute<OwnedByAttribute>()
               .EndpointName;

            return endpointName.Equals(endpointIdentity.LogicalName, StringComparison.OrdinalIgnoreCase)
                || endpointName.Equals(nameof(EndpointIdentity), StringComparison.OrdinalIgnoreCase);
        }
    }
}