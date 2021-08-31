namespace SpaceEngineers.Core.Modules.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;

    [ManuallyRegisteredComponent(nameof(DependencyContainerOverridesTest.OverrideDecoratorTest))]
    internal class ScopedCollectionResolvableDecorator : IScopedCollectionResolvable, IDecorator<IScopedCollectionResolvable>
    {
        public ScopedCollectionResolvableDecorator(IScopedCollectionResolvable decoratee)
        {
            Decoratee = decoratee;
        }

        public IScopedCollectionResolvable Decoratee { get; }
    }
}