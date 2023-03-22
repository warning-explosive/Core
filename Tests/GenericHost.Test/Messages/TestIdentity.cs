namespace SpaceEngineers.Core.GenericHost.Test.Messages
{
    using System.Reflection;
    using GenericEndpoint.Contract;

    internal static class TestIdentity
    {
        public const string Endpoint1 = nameof(Endpoint1);

        public const string Endpoint2 = nameof(Endpoint2);

        public static EndpointIdentity Endpoint10 { get; } = new EndpointIdentity(Endpoint1);

        public static EndpointIdentity Endpoint20 { get; } = new EndpointIdentity(Endpoint2);

        public static Assembly Endpoint1Assembly { get; } = Assembly.GetEntryAssembly() !;

        public static Assembly Endpoint2Assembly { get; } = Assembly.GetEntryAssembly() !;
    }
}