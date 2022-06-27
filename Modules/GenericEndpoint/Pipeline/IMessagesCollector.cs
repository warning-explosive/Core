namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using System.Threading;
    using System.Threading.Tasks;
    using Messaging;

    internal interface IMessagesCollector
    {
        Task Collect(IntegrationMessage message, CancellationToken token);
    }
}