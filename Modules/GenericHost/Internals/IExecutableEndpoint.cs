namespace SpaceEngineers.Core.GenericHost.Internals
{
    using System.Threading.Tasks;
    using AutoRegistration.Abstractions;
    using AutoWiringApi.Abstractions;
    using Core.GenericEndpoint;
    using Core.GenericEndpoint.Abstractions;

    internal interface IExecutableEndpoint : IResolvable
    {
        IDependencyContainer DependencyContainer { get; }

        Task InvokeMessageHandler(IntegrationMessage message, IExtendedIntegrationContext context);
    }
}