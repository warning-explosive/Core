namespace SpaceEngineers.Core.CompositionRoot.Abstractions
{
    /// <summary>
    /// Represents decorator for service or service collection
    /// </summary>
    public interface IDecorator<TResolvable>
        where TResolvable : IResolvable
    {
    }
}