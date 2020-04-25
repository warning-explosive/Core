namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoRegistration;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class WithInjectedDependencyContainerImpl : IWithInjectedDependencyContainer
    {
        public WithInjectedDependencyContainerImpl(IDependencyContainer dependencyContainer)
        {
            InjectedDependencyContainer = dependencyContainer;
        }

        public IDependencyContainer InjectedDependencyContainer { get; }
    }
}