namespace SpaceEngineers.Core.AuthEndpoint.Contract.Events
{
    using SpaceEngineers.Core.GenericEndpoint.Contract.Abstractions;
    using SpaceEngineers.Core.GenericEndpoint.Contract.Attributes;

    /// <summary>
    /// User created event
    /// </summary>
    [OwnedBy(AuthEndpointIdentity.LogicalName)]
    public class UserCreated : IIntegrationEvent
    {
        /// <summary> .cctor </summary>
        /// <param name="username">Username</param>
        public UserCreated(string username)
        {
            Username = username;
        }

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; init; }
    }
}