namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiring.Api.Abstractions;

    internal interface IOpenGenericDecorableServiceDecorator<T> : IDecorator<IOpenGenericDecorableService<T>>
    {
    }
}