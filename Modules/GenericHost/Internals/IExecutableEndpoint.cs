namespace SpaceEngineers.Core.GenericHost.Internals
{
    using System.Threading.Tasks;
    using Abstractions;
    using AutoRegistration.Abstractions;
    using AutoWiringApi.Abstractions;
    using Core.GenericEndpoint.Abstractions;

    internal interface IExecutableEndpoint : IResolvable
    {
        IDependencyContainer DependencyContainer { get; }

        IIntegrationTransport Transport { get; }

        Task InvokeMessageHandler(IExtendedIntegrationContext context);
    }
}