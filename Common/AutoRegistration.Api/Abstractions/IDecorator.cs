namespace SpaceEngineers.Core.AutoRegistration.Api.Abstractions
{
    /// <summary>
    /// Represents decorator for service
    /// </summary>
    /// <typeparam name="TService">TService type-argument</typeparam>
    public interface IDecorator<TService>
    {
        /// <summary>
        /// Decoratee
        /// </summary>
        TService Decoratee { get; }
    }
}