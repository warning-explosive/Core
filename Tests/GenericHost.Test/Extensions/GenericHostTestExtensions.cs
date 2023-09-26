namespace SpaceEngineers.Core.GenericHost.Test.Extensions
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using IntegrationTransport.Api;
    using Microsoft.Extensions.DependencyInjection;
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

        public static IHostBuilder UseIntegrationTransport(
            this IHostBuilder hostBuilder,
            TransportIdentity transportIdentity,
            Func<IHostBuilder, TransportIdentity, DirectoryInfo, IHostBuilder> useTransport,
            DirectoryInfo settingsDirectory)
        {
            return useTransport(hostBuilder, transportIdentity, settingsDirectory);
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
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                var hostShutdown = host.WaitForShutdownAsync(cts.Token);

                var awaiter = Task.WhenAny(producer(output, host, cts.Token), hostShutdown);

                var result = await awaiter.ConfigureAwait(false);

                if (hostShutdown == result)
                {
                    throw new InvalidOperationException("Host was unexpectedly stopped");
                }

                await result.ConfigureAwait(false);

                host
                    .Services
                    .GetRequiredService<IHostApplicationLifetime>()
                    .StopApplication();

                await hostShutdown.ConfigureAwait(false);
            }
        }
    }
}