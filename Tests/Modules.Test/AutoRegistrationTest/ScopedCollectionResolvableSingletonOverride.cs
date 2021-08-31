namespace SpaceEngineers.Core.Modules.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;

    [ComponentOverride]
    internal class ScopedCollectionResolvableSingletonOverride : IScopedCollectionResolvable, ICollectionResolvable<IScopedCollectionResolvable>
    {
    }
}