﻿namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
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

        public IntegrationMessage Message { get; set; }

        public EndpointIdentity EndpointIdentity { get; set; }

        public bool IsError { get; set; }

        public bool Handled { get; set; }
    }
}