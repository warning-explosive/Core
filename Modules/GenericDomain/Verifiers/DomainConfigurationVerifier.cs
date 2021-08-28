namespace SpaceEngineers.Core.GenericDomain.Verifiers
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions;

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