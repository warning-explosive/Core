namespace SpaceEngineers.Core.AutoRegistration.Api.Abstractions
{
    /// <summary>
    /// Represents decorator for service
    /// </summary>
    /// <typeparam name="TResolvable">IResolvable</typeparam>
    public interface IDecorator<TResolvable>
    {
        /// <summary>
        /// Decoratee
        /// </summary>
        TResolvable Decoratee { get; }
    }
}