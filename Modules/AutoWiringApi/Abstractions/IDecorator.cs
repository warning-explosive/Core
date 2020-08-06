namespace SpaceEngineers.Core.AutoWiringApi.Abstractions
{
    /// <summary>
    /// Represents decorator for service
    /// </summary>
    /// <typeparam name="TResolvable">IResolvable</typeparam>
    public interface IDecorator<TResolvable>
        where TResolvable : class
    {
        /// <summary>
        /// Decoratee
        /// </summary>
        TResolvable Decoratee { get; }
    }
}