namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using SpaceEngineers.Core.DataAccess.Api.Model;

    [SuppressMessage("Analysis", "SA1649", Justification = "StyleCop analyzer error")]
    [Schema(nameof(Deduplication))]
    internal record InboxMessage : BaseDatabaseEntity<Guid>
    {
        public InboxMessage(
            Guid primaryKey,
            IntegrationMessage message,
            EndpointIdentity endpointIdentity,
            bool isError,
            bool handled)
            : base(primaryKey)
        {
            Message = message;
            EndpointIdentity = endpointIdentity;
            IsError = isError;
            Handled = handled;
        }

        public IntegrationMessage Message { get; init; }

        public EndpointIdentity EndpointIdentity { get; init; }

        public bool IsError { get; init; }

        public bool Handled { get; init; }
    }
}