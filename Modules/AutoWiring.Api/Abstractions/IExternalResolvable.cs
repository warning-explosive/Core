namespace SpaceEngineers.Core.AutoWiring.Api.Abstractions
{
    /// <summary>
    /// Defines resolvable service for external types
    /// </summary>
    /// <typeparam name="TExternalService">TExternalService type-argument</typeparam>
    public interface IExternalResolvable<TExternalService>
        where TExternalService : class
    {
    }
}