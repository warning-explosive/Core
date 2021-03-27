namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Component(EnLifestyle.Transient, EnComponentKind.OpenGenericFallback)]
    internal class OpenGenericTestServiceImpl<T> : IOpenGenericTestService<T>
    {
    }
}