namespace SpaceEngineers.Core.CompositionRoot.Abstractions
{
    /// <summary>
    /// Represents decorator for service
    /// </summary>
    public interface IDecorator<TResolvable>
        where TResolvable : IResolvable
    {
        /// <summary>
        /// Decoratee
        /// </summary>
        TResolvable Decoratee { get; }
    }
}