namespace SpaceEngineers.Core.GenericDomain.Internals
{
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using AutoWiring.Api.Services;

    /// <summary>
    /// Verifies domain configuration
    /// </summary>
    [Component(EnLifestyle.Singleton)]
    internal class DomainConfigurationVerifier : IConfigurationVerifier,
                                                 ICollectionResolvable<IConfigurationVerifier>
    {
        /// <inheritdoc />
        public void Verify()
        {
            /*
             * TODO: implement domain checks
             * 1. sealed/immutable objects
             *    - EnumerationObject (sealed)
             *    - value-objects (record, sealed)
             * 2. properties with private setter
             */
        }
    }
}