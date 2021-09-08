namespace SpaceEngineers.Core.GenericEndpoint.Defaults
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    /// <summary>
    /// Default retry policy implementation
    /// </summary>
    [Component(EnLifestyle.Singleton)]
    public class DefaultRetryPolicy : IRetryPolicy
    {
        /// <inheritdoc />
        public Task Apply(IAdvancedIntegrationContext context, Exception exception, CancellationToken token)
        {
            return context.Refuse(exception, token);
        }
    }
}