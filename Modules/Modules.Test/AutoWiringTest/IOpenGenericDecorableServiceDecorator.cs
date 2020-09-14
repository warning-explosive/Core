namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiringApi.Abstractions;

    internal interface IOpenGenericDecorableServiceDecorator<T> : IDecorator<IOpenGenericDecorableService<T>>
    {
    }
}