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
        /// Does type represent an IIntegrationQuery
        /// </summary>
        /// <param name="type">Message type</param>
        /// <returns>Message type is an IIntegrationQuery or not</returns>
        public static bool IsQuery(this Type type)
        {
            return type.IsSubclassOfOpenGeneric(typeof(IIntegrationQuery<>));
        }

        /// <summary>
        /// Does type represent an IIntegrationQuery
        /// </summary>
        /// <param name="type">Message type</param>
        /// <returns>Message type is an IIntegrationQuery or not</returns>
        public static bool IsReply(this Type type)
        {
            return typeof(IIntegrationReply).IsAssignableFrom(type);
        }

        /// <summary>
        /// Does type represent an IIntegrationReply on IIntegrationQuery
        /// </summary>
        /// <param name="reply">Reply message type</param>
        /// <param name="query">Query message type</param>
        /// <returns>Message type is an IIntegrationReply on IIntegrationQuery or not</returns>
        public static bool IsReplyOnQuery(this Type reply, Type query)
        {
            return typeof(IIntegrationReply).IsAssignableFrom(reply)
                && query.IsSubclassOfOpenGeneric(typeof(IIntegrationQuery<>))
                && typeof(IIntegrationQuery<>).MakeGenericType(reply).IsAssignableFrom(query);
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
                   || typeof(IIntegrationQuery<>) == type.GenericTypeDefinitionOrSelf();
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