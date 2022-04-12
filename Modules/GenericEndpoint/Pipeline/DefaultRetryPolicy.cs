namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    internal class DefaultRetryPolicy : IRetryPolicy,
                                        IResolvable<IRetryPolicy>
    {
        public Task Apply(IAdvancedIntegrationContext context, Exception exception, CancellationToken token)
        {
            return context.Refuse(exception, token);
        }
    }
}