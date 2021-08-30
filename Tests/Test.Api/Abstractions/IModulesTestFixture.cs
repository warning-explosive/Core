namespace SpaceEngineers.Core.Test.Api.Abstractions
{
    using System;
    using System.Reflection;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions;

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
        IComponentsOverride DelegateOverride(Action<IComponentsOverrideContainer> overrideAction);

        /// <summary>
        /// Setup bounded above dependency container
        /// </summary>
        /// <param name="options">Dependency container options</param>
        /// <param name="aboveAssemblies">Assemblies that limits assembly loading in dependency container</param>
        /// <returns>IDependencyContainer</returns>
        IDependencyContainer BoundedAboveContainer(DependencyContainerOptions options, params Assembly[] aboveAssemblies);

        /// <summary>
        /// Setup exactly bounded dependency container
        /// </summary>
        /// <param name="options">Dependency container options</param>
        /// <param name="exactAssemblies">Assemblies that limits assembly loading in dependency container</param>
        /// <returns>IDependencyContainer</returns>
        public IDependencyContainer ExactlyBoundedContainer(DependencyContainerOptions options, params Assembly[] exactAssemblies);

        /// <summary>
        /// Setup dependency container without assembly limitations
        /// </summary>
        /// <param name="options">Dependency container options</param>
        /// <returns>IDependencyContainer</returns>
        public IDependencyContainer CreateContainer(DependencyContainerOptions options);

        /// <summary>
        /// Setup dependency container limited by specified ITypeProvider
        /// </summary>
        /// <param name="options">Dependency container options</param>
        /// <param name="typeProvider">ITypeProvider</param>
        /// <returns>IDependencyContainer</returns>
        public IDependencyContainer CreateContainer(DependencyContainerOptions options, ITypeProvider typeProvider);
    }
}