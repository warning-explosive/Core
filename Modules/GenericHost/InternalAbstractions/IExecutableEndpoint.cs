namespace SpaceEngineers.Core.GenericHost.InternalAbstractions
{
    using System.Threading.Tasks;
    using GenericEndpoint.Abstractions;

    internal interface IExecutableEndpoint
    {
        Task InvokeMessageHandler<TMessage>(
            TMessage message,
            IExtendedIntegrationContext context)
            where TMessage : IIntegrationMessage;
    }
}