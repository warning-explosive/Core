namespace SpaceEngineers.Core.GenericHost.Internals
{
    using System.Threading.Tasks;
    using AutoWiring.Api.Abstractions;
    using Core.GenericEndpoint;

    internal interface IExecutableEndpoint : IResolvable
    {
        Task InvokeMessageHandler(IntegrationMessage message);
    }
}