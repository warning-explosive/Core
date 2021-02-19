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
        /// <param name="container">IRegistrationContainer</param>
        public void Register(IRegistrationContainer container);
    }
}