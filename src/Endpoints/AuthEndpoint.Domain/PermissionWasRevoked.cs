namespace SpaceEngineers.Core.AuthEndpoint.Domain
{
    using System;
    using System.Text.Json.Serialization;
    using GenericDomain.Api.Abstractions;

    /// <summary>
    /// PermissionWasRevoked
    /// </summary>
    public record PermissionWasRevoked : IDomainEvent<User>
    {
        /// <summary> .cctor </summary>
        [JsonConstructor]
        [Obsolete("serialization constructor")]
        public PermissionWasRevoked()
        {
            Feature = default!;
        }

        /// <summary> .cctor </summary>
        /// <param name="feature">Feature</param>
        public PermissionWasRevoked(Feature feature)
        {
            Feature = feature;
        }

        /// <summary>
        /// Feature
        /// </summary>
        public Feature Feature { get; init; }
    }
}