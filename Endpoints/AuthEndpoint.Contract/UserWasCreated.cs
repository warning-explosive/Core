namespace SpaceEngineers.Core.AuthEndpoint.Contract
{
    using SpaceEngineers.Core.GenericEndpoint.Contract.Abstractions;
    using SpaceEngineers.Core.GenericEndpoint.Contract.Attributes;

    /// <summary>
    /// User was created event
    /// </summary>
    [OwnedBy(Identity.LogicalName)]
    [Feature(Features.Authentication)]
    public record UserWasCreated : IIntegrationEvent
    {
        /// <summary> .cctor </summary>
        /// <param name="username">Username</param>
        public UserWasCreated(string username)
        {
            Username = username;
        }

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; init; }
    }
}