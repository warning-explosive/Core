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
             * 1. Messages must be marked with MessageOwnerAttribute
             * 2. Message implements only one interface (command, query, event, message)
             * 3. Queries handlers must call reply
             */
        }
    }
}