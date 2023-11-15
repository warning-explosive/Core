namespace SpaceEngineers.Core.CompositionRoot.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;

    [ComponentOverride]
    internal class ScopedCollectionResolvableDecoratorOverride : IScopedCollectionResolvable, IDecorator<IScopedCollectionResolvable>
    {
        public ScopedCollectionResolvableDecoratorOverride(IScopedCollectionResolvable decoratee)
        {
            Decoratee = decoratee;
        }

        public IScopedCollectionResolvable Decoratee { get; }
    }
}