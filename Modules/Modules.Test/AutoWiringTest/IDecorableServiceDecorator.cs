namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiringApi.Abstractions;

    internal interface IDecorableServiceDecorator : IDecorableService,
                                                    IDecorator<IDecorableService>
    {
    }
}