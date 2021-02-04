namespace SpaceEngineers.Core.GenericHost.Abstractions
{
    using System.Threading.Tasks;
    using GenericEndpoint.Abstractions;

    /// <summary>
    /// Generic endpoint abstraction
    /// </summary>
    public interface IGenericEndpoint
    {
        /// <summary>
        /// Endpoint identity
        /// </summary>
        EndpointIdentity Identity { get; }

        /// <summary>
        /// Integration types provider
        /// </summary>
        IIntegrationTypesProvider IntegrationTypesProvider { get; }

        /// <summary> Build and invoke integration message handler </summary>
        /// <param name="message">Integration message</param>
        /// <param name="context">Integration context</param>
        /// <typeparam name="TMessage">TMessage type-argument</typeparam>
        /// <returns>Ongoing message handler operation</returns>
        Task InvokeMessageHandler<TMessage>(
            TMessage message,
            IIntegrationContext context)
            where TMessage : IIntegrationMessage;
    }
}