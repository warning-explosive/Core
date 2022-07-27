namespace SpaceEngineers.Core.TracingEndpoint.DatabaseModel
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Contract;
    using DataAccess.Api.Model;

    [SuppressMessage("Analysis", "SA1649", Justification = "StyleCop analyzer error")]
    [Schema(nameof(GenericEndpoint.Tracing))]
    internal record IntegrationMessage : BaseDatabaseEntity<Guid>
    {
        public IntegrationMessage(
            Guid primaryKey,
            Guid messageId,
            Guid conversationId,
            Guid? initiatorMessageId,
            JsonObject payload)
            : base(primaryKey)
        {
            MessageId = messageId;
            ConversationId = conversationId;
            InitiatorMessageId = initiatorMessageId;
            Payload = payload;
        }

        public Guid MessageId { get; set; }

        public Guid ConversationId { get; set; }

        public Guid? InitiatorMessageId { get; set; }

        public JsonObject Payload { get; set; }

        public SerializedIntegrationMessage BuildSerializedIntegrationMessage()
        {
            return new SerializedIntegrationMessage(MessageId, ConversationId, InitiatorMessageId, Payload.Value);
        }

        public static IntegrationMessage Build(SerializedIntegrationMessage serializedMessage)
        {
            return new IntegrationMessage(
                Guid.NewGuid(),
                serializedMessage.Id,
                serializedMessage.ConversationId,
                serializedMessage.InitiatorMessageId,
                new JsonObject(serializedMessage.Payload, typeof(GenericEndpoint.Messaging.IntegrationMessage)));
        }
    }
}