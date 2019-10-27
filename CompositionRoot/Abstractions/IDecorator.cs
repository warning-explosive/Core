namespace SpaceEngineers.Core.CompositionRoot.Abstractions
{
    /// <summary>
    /// Represents decorator for service
    /// </summary>
    public interface IDecorator<TResolvable>
        where TResolvable : IResolvable
    {
        TResolvable Decoratee { get; }
    }
}