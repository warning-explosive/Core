namespace SpaceEngineers.Core.GenericHost.Benchmark.Sources
{
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Contract.Attributes;

    /// <summary>
    /// Request
    /// </summary>
    [OwnedBy(nameof(MessageHandlerMiddlewareBenchmarkSource))]
    [Feature(nameof(MessageHandlerMiddlewareBenchmarkSource))]
    public record Request : IIntegrationRequest<Reply>
    {
    }
}