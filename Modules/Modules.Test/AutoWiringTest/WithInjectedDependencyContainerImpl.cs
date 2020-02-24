namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoRegistration;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class WithInjectedDependencyContainerImpl : IWithInjectedDependencyContainer
    {
        public WithInjectedDependencyContainerImpl(DependencyContainer dependencyContainer)
        {
            InjectedDependencyContainer = dependencyContainer;
        }

        public DependencyContainer InjectedDependencyContainer { get; }
    }
}