namespace SpaceEngineers.Core.IntegrationTransport.Host.Registrations
{
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Registration;
    using GenericEndpoint.RpcRequest;

    internal class RpcRequestRegistryManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.Register<IRpcRequestRegistry, RpcRequestRegistry>(EnLifestyle.Singleton);
        }
    }
}