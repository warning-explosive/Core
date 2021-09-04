namespace SpaceEngineers.Core.GenericEndpoint.Abstractions
{
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// IMessagePipelineStep abstraction
    /// </summary>
    public interface IMessagePipelineStep : IDecorator<IMessagePipeline>
    {
    }
}