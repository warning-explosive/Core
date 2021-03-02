namespace SpaceEngineers.Core.GenericEndpoint.Abstractions
{
    using AutoWiring.Api.Abstractions;

    /// <summary>
    /// IMessagePipelineStep abstraction
    /// </summary>
    public interface IMessagePipelineStep : IDecorator<IMessagePipeline>
    {
    }
}