namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    [OpenGenericFallBack]
    internal class OpenGenericTestServiceImpl<T> : IOpenGenericTestService<T>
    {
    }
}