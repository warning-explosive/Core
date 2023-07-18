namespace SpaceEngineers.Core.GenericEndpoint.Telemetry
{
    using System.Collections.Generic;
    using Messaging;
    using Messaging.MessageHeaders;
    using OpenTelemetry;
    using OpenTelemetry.Context.Propagation;
    using OpenTelemetry.Trace;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    internal class TraceContextPropagationProvider : IIntegrationMessageHeaderProvider,
                                                     ICollectionResolvable<IIntegrationMessageHeaderProvider>
    {
        public void WriteHeaders(IntegrationMessage generalMessage, IntegrationMessage? initiatorMessage)
        {
            Propagators.DefaultTextMapPropagator.Inject(
                new PropagationContext(Tracer.CurrentSpan.Context, Baggage.Current),
                generalMessage,
                InjectTraceContext);
        }

        private static void InjectTraceContext(IntegrationMessage message, string key, string value)
        {
            Dictionary<string, object> traceContextAttributes;

            if (message.ReadHeader<TraceContext>() is { } traceContext)
            {
                traceContextAttributes = traceContext.Value;
            }
            else
            {
                traceContextAttributes = new Dictionary<string, object>();
                message.WriteHeader(new TraceContext(traceContextAttributes));
            }

            traceContextAttributes[key] = value;
        }
    }
}