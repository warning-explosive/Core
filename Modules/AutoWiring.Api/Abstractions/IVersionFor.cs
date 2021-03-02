namespace SpaceEngineers.Core.AutoWiring.Api.Abstractions
{
    /// <summary>
    /// Represents abstraction for service version that could changed at run time
    /// </summary>
    /// <typeparam name="TService">TService type-argument</typeparam>
    public interface IVersionFor<TService> : ICollectionResolvable<IVersionFor<TService>>
        where TService : class
    {
        /// <summary>
        /// Version of service
        /// </summary>
        TService Version { get; }
    }
}