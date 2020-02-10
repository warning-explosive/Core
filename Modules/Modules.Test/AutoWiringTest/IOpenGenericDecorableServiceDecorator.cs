namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    internal interface IOpenGenericDecorableServiceDecorator<T> : IOpenGenericDecorableService<T>
    {
        IOpenGenericDecorableService<T> Decoratee { get; }
    }
}