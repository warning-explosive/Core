namespace SpaceEngineers.Core.AuthEndpoint.Domain.Model
{
    using GenericDomain.Api.Abstractions;

    /// <summary>
    /// PermissionWasGranted
    /// </summary>
    public record PermissionWasGranted : IDomainEvent<User>
    {
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