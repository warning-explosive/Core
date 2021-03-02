namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class DerivedFromUnregisteredExternalServiceImpl : BaseUnregisteredExternalServiceImpl
    {
    }
}