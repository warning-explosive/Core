namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Component(EnLifestyle.Transient)]
    internal class OpenGenericDecorableServiceImpl<T> : IOpenGenericDecorableService<T>
    {
    }
}