namespace SpaceEngineers.Core.GenericEndpoint.Internals
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class EmptyEndpointInitializer : IEndpointInitializer
    {
        public Task Initialize(CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}