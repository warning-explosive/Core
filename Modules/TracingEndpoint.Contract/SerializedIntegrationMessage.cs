namespace SpaceEngineers.Core.TracingEndpoint.Contract
{
    using System;
    using CrossCuttingConcerns.Api.Abstractions;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.Messaging.MessageHeaders;

    /// <summary>
    /// Json object
    /// </summary>
    public class SerializedIntegrationMessage
    {
        /// <summary> .cctor </summary>
        /// <param name="id">Id</param>
        /// <param name="conversationId">Conversation id</param>
        /// <param name="initiatorMessageId">Initiator message id</param>
        /// <param name="payload">Payload</param>
        public SerializedIntegrationMessage(
            Guid id,
            Guid conversationId,
            Guid? initiatorMessageId,
            string payload)
        {
            Id = id;
            ConversationId = conversationId;
            InitiatorMessageId = initiatorMessageId;
            Payload = payload;
        }

        /// <summary>
        /// Message id
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Conversation id
        /// </summary>
        public Guid ConversationId { get; init; }

        /// <summary>
        /// Initiator message id
        /// </summary>
        public Guid? InitiatorMessageId { get; init; }

        /// <summary>
        /// Payload
        /// </summary>
        public string Payload { get; init; }

        /// <summary>
        /// Implicit conversion operator to SerializedIntegrationMessage
        /// </summary>
        /// <param name="integrationMessage">SerializedMessage</param>
        /// <param name="jsonSerializer">IJsonSerializer</param>
        /// <returns>SerializedIntegrationMessage</returns>
        public static SerializedIntegrationMessage FromIntegrationMessage(
            IntegrationMessage integrationMessage,
            IJsonSerializer jsonSerializer)
        {
            return new SerializedIntegrationMessage(
                integrationMessage.ReadRequiredHeader<Id>().Value,
                integrationMessage.ReadRequiredHeader<ConversationId>().Value,
                integrationMessage.ReadHeader<InitiatorMessageId>()?.Value,
                jsonSerializer.SerializeObject(integrationMessage));
        }

        /// <summary>
        /// Implicit conversion operator to SerializedIntegrationMessage
        /// </summary>
        /// <param name="jsonSerializer">IJsonSerializer</param>
        /// <returns>SerializedIntegrationMessage</returns>
        public IntegrationMessage ToIntegrationMessage(IJsonSerializer jsonSerializer)
        {
            return jsonSerializer.DeserializeObject<IntegrationMessage>(Payload);
        }
    }
}