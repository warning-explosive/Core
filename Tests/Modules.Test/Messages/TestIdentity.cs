namespace SpaceEngineers.Core.Modules.Test.Messages
{
    using GenericEndpoint.Contract;
    using TracingEndpoint.Contract;

    internal static class TestIdentity
    {
        public const string Endpoint1 = nameof(Endpoint1);
        public const string Endpoint2 = nameof(Endpoint2);

        public static readonly EndpointIdentity Endpoint10 = new EndpointIdentity(Endpoint1, 0);
        public static readonly EndpointIdentity Endpoint11 = new EndpointIdentity(Endpoint1, 1);
        public static readonly EndpointIdentity Endpoint20 = new EndpointIdentity(Endpoint2, 0);

        public static readonly EndpointIdentity TracingEndpoint = new EndpointIdentity(TracingEndpointIdentity.LogicalName, 0);
    }
}