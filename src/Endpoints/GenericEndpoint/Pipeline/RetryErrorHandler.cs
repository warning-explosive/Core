namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    internal class RetryErrorHandler : IErrorHandler,
                                       ICollectionResolvable<IErrorHandler>
    {
        private readonly IRetryPolicy _retryPolicy;

        public RetryErrorHandler(IRetryPolicy retryPolicy)
        {
            _retryPolicy = retryPolicy;
        }

        public Task Handle(
            IAdvancedIntegrationContext context,
            Exception exception,
            CancellationToken token)
        {
            return _retryPolicy.Apply(context, exception, token);
        }
    }
}