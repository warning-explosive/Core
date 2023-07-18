namespace SpaceEngineers.Core.GenericEndpoint.Telemetry
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Attributes;
    using Messaging;
    using Messaging.MessageHeaders;
    using OpenTelemetry;
    using OpenTelemetry.Context.Propagation;
    using OpenTelemetry.Trace;
    using Pipeline;

    /// <summary>
    /// TracingMiddleware
    /// </summary>
    [Component(EnLifestyle.Singleton)]
    [Before(typeof(ErrorHandlingMiddleware))]
    public class TracingMiddleware : IMessageHandlerMiddleware,
                                     ICollectionResolvable<IMessageHandlerMiddleware>
    {
        private readonly Tracer _tracer;

        /// <summary> .cctor </summary>
        /// <param name="tracer">Tracer</param>
        public TracingMiddleware(
            Tracer tracer)
        {
            _tracer = tracer;
        }

        /// <inheritdoc />
        [SuppressMessage("Analysis", "CA1308", Justification = "conventional attributes")]
        public async Task Handle(
            IAdvancedIntegrationContext context,
            Func<IAdvancedIntegrationContext, CancellationToken, Task> next,
            CancellationToken token)
        {
            var parentContext = Propagators.DefaultTextMapPropagator.Extract(default, context.Message, ExtractTraceContext);
            Baggage.Current = parentContext.Baggage;

            var attributes = context
                .Message
                .Headers
                .Select(pair => new KeyValuePair<string, object>($"messaging.message.header.{pair.Key.Name.ToLowerInvariant()}", pair.Value.StringValue));

            using (_tracer.StartActiveSpan("MessageHandler", SpanKind.Server, new SpanContext(parentContext.ActivityContext), new SpanAttributes(attributes)))
            {
                await next(context, token).ConfigureAwait(false);
            }
        }

        private static IEnumerable<string> ExtractTraceContext(IntegrationMessage message, string key)
        {
            if (message.ReadRequiredHeader<TraceContext>().Value.TryGetValue(key, out var value))
            {
                yield return (string)value;
            }
        }
    }
}