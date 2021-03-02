namespace SpaceEngineers.Core.AutoWiring.Api.Abstractions
{
    /// <summary>
    /// IExternalResolvable service - wrapper for registration external services
    /// </summary>
    /// <typeparam name="TExternalService">External service type-argument</typeparam>
    public interface IExternalResolvable<TExternalService>
        where TExternalService : class
    {
    }
}