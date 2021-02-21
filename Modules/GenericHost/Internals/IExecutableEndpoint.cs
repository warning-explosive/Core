namespace SpaceEngineers.Core.GenericHost.Internals
{
    using System.Threading.Tasks;
    using Core.GenericEndpoint;
    using Core.GenericEndpoint.Abstractions;

    internal interface IExecutableEndpoint
    {
        Task InvokeMessageHandler(IntegrationMessage message, IExtendedIntegrationContext context);
    }
}