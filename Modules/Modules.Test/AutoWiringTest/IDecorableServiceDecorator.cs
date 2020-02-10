namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    internal interface IDecorableServiceDecorator : IDecorableService
    {
        IDecorableService Decoratee { get; }
    }
}