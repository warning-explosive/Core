namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class DerivedFromUnregisteredServiceImpl : BaseUnregisteredServiceImpl
    {
    }
}