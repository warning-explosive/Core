namespace SpaceEngineers.Core.CompositionRoot.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Scoped)]
    internal class ScopedCollectionResolvable : IScopedCollectionResolvable, ICollectionResolvable<IScopedCollectionResolvable>
    {
    }
}