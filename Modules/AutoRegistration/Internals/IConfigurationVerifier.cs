namespace SpaceEngineers.Core.AutoRegistration.Internals
{
    using AutoWiringApi.Abstractions;

    internal interface IConfigurationVerifier : ICollectionResolvable
    {
        void Verify();
    }
}