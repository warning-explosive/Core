namespace SpaceEngineers.Core.GenericEndpoint.Defaults
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Basics;

    /// <summary>
    /// Default IRetryPolicy implementation
    /// </summary>
    [Lifestyle(EnLifestyle.Singleton)]
    public class DefaultRetryPolicy : IRetryPolicy
    {
        /// <inheritdoc />
        public Task Apply(IntegrationMessage message, IExtendedIntegrationContext context, CancellationToken token)
        {
            return context
                .CallMethod(nameof(IExtendedIntegrationContext.Retry))
                .WithTypeArgument(message.ReflectedType)
                .WithArguments(message.Message, token)
                .Invoke<Task>();
        }
    }
}