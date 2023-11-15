namespace SpaceEngineers.Core.GenericHost.Benchmark.Sources
{
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Contract.Attributes;

    /// <summary>
    /// Reply
    /// </summary>
    [Feature(nameof(MessageHandlerMiddlewareBenchmarkSource))]
    public record Reply : IIntegrationReply
    {
    }
}