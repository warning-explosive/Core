namespace SpaceEngineers.Core.GenericHost.Endpoint
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Core.GenericEndpoint.Abstractions;

    /// <summary>
    /// Default IRetryPolicy implementation
    /// </summary>
    [Lifestyle(EnLifestyle.Singleton)]
    public class DefaultRetryPolicy : IRetryPolicy
    {
        /// <inheritdoc />
        public Task Apply<TMessage>(TMessage message, IExtendedIntegrationContext context, CancellationToken token)
            where TMessage : IIntegrationMessage
        {
            return context.Retry(message, token);
        }
    }
}