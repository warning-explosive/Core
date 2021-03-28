namespace SpaceEngineers.Core.GenericEndpoint.Executable.Internals
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoWiring.Api.Abstractions;

    internal interface IRunnableEndpoint : IResolvable
    {
        Task StartAsync(CancellationToken token);

        Task StopAsync();
    }
}