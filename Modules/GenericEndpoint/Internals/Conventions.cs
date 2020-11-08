namespace SpaceEngineers.Core.GenericEndpoint.Internals
{
    internal static class Conventions
    {
        internal static string InputQueueName(string endpointName) => $"{endpointName}.Input";
    }
}