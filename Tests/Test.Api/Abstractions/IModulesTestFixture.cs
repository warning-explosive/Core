namespace SpaceEngineers.Core.Test.Api.Abstractions
{
    using System;
    using System.Reflection;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions.Container;
    using CompositionRoot.Api.Abstractions.Registration;
    using Xunit.Abstractions;

    /// <summary>
    /// IModulesTestFixture
    /// </summary>
    public interface IModulesTestFixture
    {
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
        /// Creates IDependencyContainer bounded above by specified assemblies
        /// </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="options">Dependency container options</param>
        /// <param name="aboveAssemblies">Assemblies that limits assembly loading in dependency container</param>
        /// <returns>IDependencyContainer</returns>
        IDependencyContainer BoundedAboveContainer(
            ITestOutputHelper output,
            DependencyContainerOptions options,
            params Assembly[] aboveAssemblies);

        /// <summary>
        /// Creates IDependencyContainer exactly bounded by specified assemblies
        /// </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="options">Dependency container options</param>
        /// <param name="exactAssemblies">Assemblies that limits assembly loading in dependency container</param>
        /// <returns>IDependencyContainer</returns>
        public IDependencyContainer ExactlyBoundedContainer(
            ITestOutputHelper output,
            DependencyContainerOptions options,
            params Assembly[] exactAssemblies);

        /// <summary>
        /// Creates IDependencyContainer without assembly limitations
        /// </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="options">Dependency container options</param>
        /// <returns>IDependencyContainer</returns>
        public IDependencyContainer Container(
            ITestOutputHelper output,
            DependencyContainerOptions options);
    }
}