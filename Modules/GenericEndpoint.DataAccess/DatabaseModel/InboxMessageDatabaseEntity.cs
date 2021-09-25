namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.DatabaseModel
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Core.DataAccess.Api.Model;

    [SuppressMessage("Analysis", "SA1649", Justification = "StyleCop analyzer error")]
    internal record InboxMessageDatabaseEntity : BaseDatabaseEntity<Guid>
    {
        public InboxMessageDatabaseEntity(
            Guid primaryKey,
            IntegrationMessageDatabaseEntity message,
            EndpointIdentityInlinedObject endpointIdentity,
            bool isError,
            bool handled)
            : base(primaryKey)
        {
            Message = message;
            EndpointIdentity = endpointIdentity;
            IsError = isError;
            Handled = handled;
        }

        public IntegrationMessageDatabaseEntity Message { get; private init; }

        public EndpointIdentityInlinedObject EndpointIdentity { get; private init; }

        public bool IsError { get; private init; }

        public bool Handled { get; private init; }
    }
}