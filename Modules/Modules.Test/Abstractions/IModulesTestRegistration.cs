namespace SpaceEngineers.Core.Modules.Test.Abstractions
{
    using AutoRegistration.Abstractions;

    /// <summary>
    /// ITestClassWithRegistration
    /// </summary>
    public interface IModulesTestRegistration
    {
        /// <summary>
        /// Register services for test purposes
        /// </summary>
        /// <param name="registration">IRegistrationContainer</param>
        void Register(IRegistrationContainer registration);
    }
}