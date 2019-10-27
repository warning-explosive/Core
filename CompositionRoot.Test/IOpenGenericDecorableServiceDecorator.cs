namespace SpaceEngineers.Core.CompositionRoot.Test
{
    internal interface IOpenGenericDecorableServiceDecorator<T> : IOpenGenericDecorableService<T>
    {
        IOpenGenericDecorableService<T> Decoratee { get; }
    }
}