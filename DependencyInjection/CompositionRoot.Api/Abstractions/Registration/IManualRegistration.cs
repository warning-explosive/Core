namespace SpaceEngineers.Core.CompositionRoot.Api.Abstractions.Registration
{
    /// <summary>
    /// IManualRegistration
    /// </summary>
    public interface IManualRegistration
    {
        /// <summary>
        /// Registers dependencies manually
        /// </summary>
        /// <param name="container">IManualRegistrationsContainer</param>
        public void Register(IManualRegistrationsContainer container);
    }
}