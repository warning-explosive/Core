namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiringApi.Abstractions;

    internal interface IVersionedAndDecoratedDecorator : IVersionedAndDecorated,
                                                         IDecorator<IVersionedAndDecorated>
    {
    }
}