namespace SpaceEngineers.Core.CompositionRoot.Test.AutoRegistrationTest
{
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;

    [ComponentOverride]
    internal class ScopedCollectionResolvableDecoratorTransientOverride : IScopedCollectionResolvable, IDecorator<IScopedCollectionResolvable>
    {
        public ScopedCollectionResolvableDecoratorTransientOverride(IScopedCollectionResolvable decoratee)
        {
            Decoratee = decoratee;
        }

        public IScopedCollectionResolvable Decoratee { get; }
    }
}