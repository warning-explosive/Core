namespace SpaceEngineers.Core.Modules.Test.Messages
{
    using GenericEndpoint.Contract;

    internal static class TestIdentity
    {
        public const string Endpoint1 = nameof(Endpoint1);
        public const string Endpoint2 = nameof(Endpoint2);

        public const string Instance0 = nameof(Instance0);

        public static readonly EndpointIdentity Endpoint10 = new EndpointIdentity(Endpoint1, Instance0);
        public static readonly EndpointIdentity Endpoint20 = new EndpointIdentity(Endpoint2, Instance0);
    }
}