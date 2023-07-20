namespace SpaceEngineers.Core.GenericHost.Test.Extensions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using IntegrationTransport.Api;
    using Microsoft.Extensions.Hosting;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    internal static class GenericHostTestExtensions
    {
        public static IHostBuilder UseIntegrationTransport(
            this IHostBuilder hostBuilder,
            TransportIdentity transportIdentity,
            Func<IHostBuilder, TransportIdentity, IHostBuilder> useTransport)
        {
            return useTransport(hostBuilder, transportIdentity);
        }

        internal static async Task RunTestHost(
            this IHost host,
            ITestOutputHelper output,
            IXunitTestCase testCase,
            Func<ITestOutputHelper, IHost, CancellationToken, Task> producer)
        {
            using (host)
            using (var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(testCase.Timeout)))
            {
                // TODO: var waitUntilTransportIsNotRunning = host.WaitUntilTransportIsNotRunning(cts.Token);
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                var hostShutdown = host.WaitForShutdownAsync(cts.Token);

                // TODO: await waitUntilTransportIsNotRunning.ConfigureAwait(false);
                var awaiter = Task.WhenAny(producer(output, host, cts.Token), hostShutdown);

                var result = await awaiter.ConfigureAwait(false);

                if (hostShutdown == result)
                {
                    throw new InvalidOperationException("Host was unexpectedly stopped");
                }

                await result.ConfigureAwait(false);

                await host.StopAsync(cts.Token).ConfigureAwait(false);

                await hostShutdown.ConfigureAwait(false);
            }
        }
    }
}