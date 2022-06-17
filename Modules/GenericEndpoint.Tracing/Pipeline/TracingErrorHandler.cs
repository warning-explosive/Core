namespace SpaceEngineers.Core.GenericEndpoint.Tracing.Pipeline
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Attributes;
    using Contract;
    using CrossCuttingConcerns.Json;
    using Extensions;
    using GenericEndpoint.Pipeline;
    using Messaging.Abstractions;

    [Component(EnLifestyle.Singleton)]
    [Dependent(typeof(RetryErrorHandler))]
    internal class TracingErrorHandler : IErrorHandler,
                                         ICollectionResolvable<IErrorHandler>
    {
        private readonly EndpointIdentity _endpointIdentity;
        private readonly IIntegrationMessageFactory _integrationMessageFactory;
        private readonly IJsonSerializer _jsonSerializer;

        public TracingErrorHandler(
            EndpointIdentity endpointIdentity,
            IIntegrationMessageFactory integrationMessageFactory,
            IJsonSerializer jsonSerializer)
        {
            _endpointIdentity = endpointIdentity;
            _integrationMessageFactory = integrationMessageFactory;
            _jsonSerializer = jsonSerializer;
        }

        public Task Handle(
            IAdvancedIntegrationContext context,
            Exception exception,
            CancellationToken token)
        {
            return context.CaptureTrace(
                _endpointIdentity,
                exception,
                _integrationMessageFactory,
                _jsonSerializer,
                token);
        }
    }
}