namespace SpaceEngineers.Core.AutoWiringApi.Abstractions
{
    /// <summary>
    /// IConfigurationVerifier
    /// </summary>
    public interface IConfigurationVerifier : ICollectionResolvable<IConfigurationVerifier>
    {
        /// <summary>
        /// Verify container configuration
        /// </summary>
        void Verify();
    }
}