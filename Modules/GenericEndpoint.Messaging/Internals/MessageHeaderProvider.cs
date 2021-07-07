namespace SpaceEngineers.Core.GenericEndpoint.Messaging.Internals
{
    using System.Collections.Generic;
    using Abstractions;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    internal class MessageHeaderProvider : IMessageHeaderProvider,
                                           ICollectionResolvable<IMessageHeaderProvider>
    {
        public IReadOnlyCollection<string> ForAutomaticForwarding { get; }
            = new[]
            {
                IntegrationMessageHeader.ConversationId,
                IntegrationMessageHeader.RetryCounter
            };
    }
}