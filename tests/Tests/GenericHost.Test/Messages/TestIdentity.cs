namespace SpaceEngineers.Core.GenericHost.Test.Messages
{
    using System;
    using System.Reflection;
    using GenericEndpoint.Contract;

    internal static class TestIdentity
    {
        public const string Endpoint1 = nameof(Endpoint1);

        public const string Endpoint2 = nameof(Endpoint2);

        public static Assembly Endpoint1Assembly { get; } = Assembly.GetEntryAssembly() ?? throw new InvalidOperationException("Unable to get entry assembly");

        public static Assembly Endpoint2Assembly { get; } = Assembly.GetEntryAssembly() ?? throw new InvalidOperationException("Unable to get entry assembly");

        public static EndpointIdentity Endpoint10 { get; } = new EndpointIdentity(Endpoint1, Endpoint1Assembly);

        public static EndpointIdentity Endpoint20 { get; } = new EndpointIdentity(Endpoint2, Endpoint2Assembly);
    }
}