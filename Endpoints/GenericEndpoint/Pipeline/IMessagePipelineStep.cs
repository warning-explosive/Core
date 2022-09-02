namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// IMessagePipelineStep abstraction
    /// </summary>
    public interface IMessagePipelineStep : IDecorator<IMessagePipeline>
    {
    }
}