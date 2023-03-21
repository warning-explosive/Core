namespace SpaceEngineers.Core.GenericHost.Test.Messages
{
    using System;
    using GenericEndpoint.Contract;

    internal static class TestIdentity
    {
        public const string Endpoint1 = nameof(Endpoint1);

        public const string Endpoint2 = nameof(Endpoint2);

        public static string Instance0 { get; } = Guid.NewGuid().ToString();

        public static string Instance1 { get; } = Guid.NewGuid().ToString();

        public static EndpointIdentity Endpoint10 { get; } = new EndpointIdentity(Endpoint1, Instance0);

        public static EndpointIdentity Endpoint20 { get; } = new EndpointIdentity(Endpoint2, Instance0);
    }
}