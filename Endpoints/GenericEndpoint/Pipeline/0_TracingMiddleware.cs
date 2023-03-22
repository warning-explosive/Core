namespace SpaceEngineers.Core.GenericEndpoint.Pipeline;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoRegistration.Api.Abstractions;
using AutoRegistration.Api.Attributes;
using AutoRegistration.Api.Enumerations;
using Endpoint;
using OpenTelemetry.Trace;

/// <summary>
/// TracingMiddleware
/// </summary>
[Component(EnLifestyle.Singleton)]
public class TracingMiddleware : IMessageHandlerMiddleware,
                                 ICollectionResolvable<IMessageHandlerMiddleware>
{
    private readonly ITelemetry _telemetry;

    /// <summary> .cctor </summary>
    /// <param name="telemetry">ITelemetry</param>
    public TracingMiddleware(
        ITelemetry telemetry)
    {
        _telemetry = telemetry;
    }

    /// <inheritdoc />
    public async Task Handle(
        IAdvancedIntegrationContext context,
        Func<IAdvancedIntegrationContext, CancellationToken, Task> next,
        CancellationToken token)
    {
        var attributes = context
            .Message
            .Headers
            .Select(pair => new KeyValuePair<string, object>($"Message.{pair.Key.Name}", pair.Value.StringValue));

        using (_telemetry.Tracer.StartActiveSpan("MessageHandler", SpanKind.Server, initialAttributes: new SpanAttributes(attributes)))
        {
            await next(context, token).ConfigureAwait(false);
        }
    }
}