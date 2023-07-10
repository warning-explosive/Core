namespace SpaceEngineers.Core.AuthEndpoint.Domain
{
    using System;
    using System.Text.Json.Serialization;
    using GenericDomain.Api.Abstractions;

    /// <summary>
    /// PermissionWasGranted
    /// </summary>
    public record PermissionWasGranted : IDomainEvent<User>
    {
        /// <summary> .cctor </summary>
        [JsonConstructor]
        [Obsolete("serialization constructor")]
        public PermissionWasGranted()
        {
            Feature = default!;
        }

        /// <summary> .cctor </summary>
        /// <param name="feature">Feature</param>
        public PermissionWasGranted(Feature feature)
        {
            Feature = feature;
        }

        /// <summary>
        /// Feature
        /// </summary>
        public Feature Feature { get; init; }
    }
}