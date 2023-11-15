namespace SpaceEngineers.Core.GenericEndpoint.Telemetry
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics.Attributes;
    using OpenTelemetry.Trace;
    using Pipeline;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;

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