namespace SpaceEngineers.Core.AuthEndpoint.Contract.Queries
{
    using System.Collections.Generic;
    using Replies;
    using SpaceEngineers.Core.GenericEndpoint.Contract.Abstractions;
    using SpaceEngineers.Core.GenericEndpoint.Contract.Attributes;

    /// <summary>
    /// Authorize user query
    /// </summary>
    /*TODO: #204 -> add features support for mq inter-communications*/
    [OwnedBy(AuthEndpointIdentity.LogicalName)]
    public record AuthorizeUser : IIntegrationQuery<UserAuthorizationResult>
    {
        /// <summary> .cctor </summary>
        /// <param name="username">Username</param>
        /// <param name="requiredFeatures">Required features</param>
        public AuthorizeUser(string username, IReadOnlyCollection<string> requiredFeatures)
        {
            Username = username;
            RequiredFeatures = requiredFeatures;
        }

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; init; }

        /// <summary>
        /// Required features
        /// </summary>
        public IReadOnlyCollection<string> RequiredFeatures { get; init; }
    }
}