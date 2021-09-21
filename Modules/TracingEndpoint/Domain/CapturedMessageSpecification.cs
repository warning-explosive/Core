namespace SpaceEngineers.Core.TracingEndpoint.Domain
{
    using System;
    using GenericDomain.Api.Abstractions;

    internal class CapturedMessageSpecification : IAggregateSpecification
    {
        public CapturedMessageSpecification(Guid integrationMessageId)
        {
            IntegrationMessageId = integrationMessageId;
        }

        public Guid IntegrationMessageId { get; }
    }
}