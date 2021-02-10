namespace SpaceEngineers.Core.GenericEndpoint.Verifiers
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class GenericEndpointConfigurationVerifier : IConfigurationVerifier
    {
        /// <inheritdoc />
        public void Verify()
        {
            /*
             * TODO: implement endpoint checks
             * Messages must be marked with MessageOwnerAttribute
             * Events must be immutable and sealed (records/sealed classes)
             */
        }
    }
}