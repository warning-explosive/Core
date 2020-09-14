namespace SpaceEngineers.Core.AutoRegistration.Implementations
{
    using AutoWiringApi.Abstractions;

    internal interface IConfigurationVerifier : ICollectionResolvable<IConfigurationVerifier>
    {
        void Verify();
    }
}