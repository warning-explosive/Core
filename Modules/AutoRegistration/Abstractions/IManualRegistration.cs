namespace SpaceEngineers.Core.AutoRegistration.Abstractions
{
    /// <summary>
    /// IManualRegistration abstraction
    /// </summary>
    public interface IManualRegistration
    {
        /// <summary>
        /// Register dependencies manually
        /// </summary>
        /// <param name="container">IManualRegistrationsContainer</param>
        public void Register(IManualRegistrationsContainer container);
    }
}