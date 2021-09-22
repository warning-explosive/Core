namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.DatabaseModel
{
    using System;
    using Core.DataAccess.Api.DatabaseEntity;

    internal class InboxMessageDatabaseEntity : BaseDatabaseEntity<Guid>
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

        public IntegrationMessageDatabaseEntity Message { get; }

        public EndpointIdentityInlinedObject EndpointIdentity { get; }

        public bool IsError { get; }

        public bool Handled { get; }
    }
}