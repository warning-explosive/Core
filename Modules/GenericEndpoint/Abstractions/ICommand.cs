namespace SpaceEngineers.Core.GenericEndpoint.Abstractions
{
    using NServiceBus;

    /// <summary>
    /// Generic command with expected response
    /// </summary>
    /// <typeparam name="TResponse">TResponse type-argument</typeparam>
    public interface ICommand<TResponse> : ICommand
        where TResponse : IMessage
    {
    }
}