namespace SpaceEngineers.Core.AuthorizationEndpoint.Contract
{
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Contract.Attributes;

    /// <summary>
    /// User created event
    /// </summary>
    [OwnedBy(AuthorizationEndpointIdentity.LogicalName)]
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