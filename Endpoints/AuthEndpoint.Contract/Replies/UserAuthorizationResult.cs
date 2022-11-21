namespace SpaceEngineers.Core.AuthEndpoint.Contract.Replies
{
    using SpaceEngineers.Core.GenericEndpoint.Contract.Abstractions;

    /// <summary>
    /// User authorization result
    /// </summary>
    public record UserAuthorizationResult : IIntegrationReply
    {
        /// <summary> .cctor </summary>
        /// <param name="accessGranted">AccessGranted</param>
        public UserAuthorizationResult(bool accessGranted)
        {
            AccessGranted = accessGranted;
        }

        /// <summary>
        /// AccessGranted
        /// </summary>
        public bool AccessGranted { get; init; }
    }
}