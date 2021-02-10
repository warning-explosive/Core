namespace SpaceEngineers.Core.GenericHost.Internals
{
    using System.Threading.Tasks;
    using Abstractions;
    using GenericEndpoint.Abstractions;

    /// <summary>
    /// Executable endpoint abstraction
    /// </summary>
    internal interface IExecutableEndpoint : IGenericEndpoint
    {
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