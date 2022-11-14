namespace SpaceEngineers.Core.WebApplication.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using AuthEndpoint.Contract;
    using Basics;
    using Basics.Primitives;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using Core.Test.WebApplication;
    using JwtAuthentication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using RestSharp;
    using Web.Auth.Extensions;
    using Xunit;
    using Xunit.Abstractions;
    using Program = Core.Test.WebApplication.Program;

    /// <summary>
    /// WebApiTest
    /// </summary>
    public class WebApiTest : TestBase
    {
        /// <summary> .cctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">TestFixture</param>
        public WebApiTest(ITestOutputHelper output, TestFixture fixture)
            : base(output, fixture)
        {
        }

        /// <summary>
        /// WebControllerTestData
        /// </summary>
        /// <returns>Test data</returns>
        public static IEnumerable<object?[]> WebControllerTestData()
        {
            var hosts = WebControllerTestHosts().ToArray();
            var testCases = WebControllerTestCases().ToArray();
            var countdownEvent = new AsyncCountdownEvent(testCases.Length);

            return hosts
               .SelectMany(host => testCases
                   .Select(testCase => host
                       .Concat(new object[] { countdownEvent })
                       .Concat(testCase)
                       .ToArray()));
        }

        internal static IEnumerable<object[]> WebControllerTestHosts()
        {
            var timeout = TimeSpan.FromSeconds(60);

            var cts = new CancellationTokenSource(timeout);

            var host = new Lazy<IHost>(() =>
                {
                    var host = Program.BuildHost(Array.Empty<string>());

                    host.StartAsync(cts.Token).Wait(cts.Token);

                    return host;
                },
                LazyThreadSafetyMode.ExecutionAndPublication);

            yield return new object[] { host, cts };
        }

        internal static IEnumerable<object?[]> WebControllerTestCases()
        {
            var username = "qwerty";
            var password = "12345678";

            var authEndpointConfigurationFilePath = SolutionExtensions
               .SolutionFile()
               .Directory
               .EnsureNotNull("Solution directory wasn't found")
               .StepInto("Tests")
               .StepInto("Test.WebApplication")
               .StepInto("Settings")
               .StepInto(AuthEndpointIdentity.LogicalName)
               .GetFile("appsettings", ".json")
               .FullName;

            var authEndpointConfiguration = new ConfigurationBuilder()
               .AddJsonFile(authEndpointConfigurationFilePath)
               .Build();

            var tokenProvider = new JwtTokenProvider(authEndpointConfiguration.GetJwtAuthenticationConfiguration());

            yield return new object?[]
            {
                $"http://127.0.0.1:5000/Test/{nameof(TestController.ApplicationInfo)}",
                default(string?)
            };

            yield return new object?[]
            {
                $"http://127.0.0.1:5000/Test/{nameof(TestController.Username)}",
                $"Basic {(username, password).EncodeBasicAuth()}"
            };

            yield return new object?[]
            {
                $"http://127.0.0.1:5000/Test/{nameof(TestController.FakePost)}/List",
                $"Bearer {tokenProvider.GenerateToken(username, TimeSpan.FromMinutes(1))}"
            };
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(WebControllerTestData))]
        internal async Task WebControllerTest(
            Lazy<IHost> host,
            CancellationTokenSource cts,
            AsyncCountdownEvent asyncCountdownEvent,
            string url,
            string? authorizationToken)
        {
            try
            {
                Output.WriteLine(url);

                var hostShutdown = host.Value.WaitForShutdownAsync(cts.Token);

                using (var client = new RestClient())
                {
                    var request = new RestRequest(url, Method.Get);
                    request.AddHeader("Cache-Control", "no-cache");

                    if (!authorizationToken.IsNullOrEmpty())
                    {
                        request.AddHeader("Authorization", authorizationToken);
                    }

                    request.Timeout = 10_000;

                    var awaiter = Task.WhenAny(
                        hostShutdown,
                        client.GetAsync(request, cts.Token));

                    var result = await awaiter.ConfigureAwait(false);

                    if (hostShutdown == result)
                    {
                        throw new InvalidOperationException("Host was unexpectedly stopped");
                    }

                    var response = await ((Task<RestResponse>)result).ConfigureAwait(false);

                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.NotNull(response.Content);

                    Output.WriteLine(response.Content);
                }
            }
            finally
            {
                asyncCountdownEvent.Decrement();

                if (asyncCountdownEvent.Read() == 0)
                {
                    Output.WriteLine("CLEANUP");

                    try
                    {
                        await host
                           .Value
                           .StopAsync(cts.Token)
                           .ConfigureAwait(false);
                    }
                    finally
                    {
                        cts.Dispose();
                        host.Value.Dispose();
                    }
                }
            }
        }
    }
}