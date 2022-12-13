namespace SpaceEngineers.Core.AuthEndpoint.Domain.Model
{
    using GenericDomain.Api.Abstractions;

    /// <summary>
    /// PermissionWasRevoked
    /// </summary>
    public record PermissionWasRevoked : IDomainEvent<User>
    {
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