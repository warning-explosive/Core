namespace SpaceEngineers.Core.GenericDomain
{
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    /// <summary>
    /// Verifies domain configuration
    /// </summary>
    [Lifestyle(EnLifestyle.Singleton)]
    internal class DomainConfigurationVerifier : IConfigurationVerifier
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