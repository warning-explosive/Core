namespace SpaceEngineers.Core.GenericEndpoint.Abstractions
{
    using AutoWiringApi.Abstractions;

    /// <summary>
    /// IMessagePipelineStep abstraction
    /// </summary>
    public interface IMessagePipelineStep : IDecorator<IMessagePipeline>
    {
    }
}