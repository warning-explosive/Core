namespace SpaceEngineers.Core.Test.Api.Abstractions
{
    using System;
    using System.Reflection;
    using CompositionRoot;
    using CompositionRoot.Registration;
    using Microsoft.Extensions.Hosting;
    using Xunit.Abstractions;

    /// <summary>
    /// IModulesTestFixture
    /// </summary>
    public interface IModulesTestFixture
    {
        /// <summary>
        /// Creates and configures IHostBuilder
        /// </summary>
        /// <returns>IHostBuilder</returns>
        IHostBuilder CreateHostBuilder();

        /// <summary>
        /// Generates IManualRegistration object with specified delegate
        /// </summary>
        /// <param name="registrationAction">Registration action</param>
        /// <returns>IManualRegistration</returns>
        IManualRegistration DelegateRegistration(Action<IManualRegistrationsContainer> registrationAction);

        /// <summary>
        /// Generates IComponentsOverride object with specified delegate
        /// </summary>
        /// <param name="overrideAction">Override action</param>
        /// <returns>IComponentsOverride</returns>
        IComponentsOverride DelegateOverride(Action<IRegisterComponentsOverrideContainer> overrideAction);

        /// <summary>
        /// Creates IDependencyContainer
        /// </summary>
        /// <param name="options">Dependency container options</param>
        /// <returns>IDependencyContainer</returns>
        public IDependencyContainer DependencyContainer(DependencyContainerOptions options);
    }
}