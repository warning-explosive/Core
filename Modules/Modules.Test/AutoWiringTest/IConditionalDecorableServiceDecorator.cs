namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    internal interface IConditionalDecorableServiceDecorator : IConditionalDecorableService
    {
        IConditionalDecorableService Decoratee { get; }
    }
}