namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Attributes;
    using OpenTelemetry.Trace;

    [Component(EnLifestyle.Singleton)]
    [After(typeof(RetryErrorHandler))]
    internal class TracingErrorHandler : IErrorHandler,
                                         ICollectionResolvable<IErrorHandler>
    {
        public Task Handle(
            IAdvancedIntegrationContext context,
            Exception exception,
            CancellationToken token)
        {
            Tracer.CurrentSpan.SetStatus(Status.Error);
            Tracer.CurrentSpan.RecordException(exception);

            return Task.CompletedTask;
        }
    }
}