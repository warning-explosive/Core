namespace SpaceEngineers.Core.Modules.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;

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