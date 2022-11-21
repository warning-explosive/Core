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
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using RestSharp;
    using Web.Api.Api;
    using Web.Auth;
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

            /*
             * Authorization = authentication + permissions
             *    1. Authenticate user; call IdentityProvider and receive identity token;
             *    2. Call AuthorizationProvider and receive API/Service/UI-specific roles and permissions;
             *
             * AuthorizationProvider maps identity data (who) to authorization data (permission and roles - what principal can do)
             * Mappings can be static (common rules, conditions and conventions) or dynamic (claims transformations)
             * Each application (web-api endpoint, micro-service, UI application, etc.) should have their scoped roles, permissions and mapping rules.
             * Application can register authorization client so as to ask for authorization data from authorization service.
             * For dynamically defined policies we can use custom PolicyProvider for policy-based authorization (ASP.NET CORE) and inject policy name into AuthorizeAttribute.
             */

            yield return new object?[]
            {
                new RestRequest($"http://127.0.0.1:5000/api/Auth/{nameof(AuthController.AuthenticateUser)}", Method.Get)
                    .AddHeader("Authorization", $"Basic {(username, password).EncodeBasicAuth()}"),
                new Action<RestResponse, ITestOutputHelper>(static (response, output) =>
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.NotNull(response.Content);
                    output.WriteLine(response.Content);

                    var token = JsonConvert
                        .DeserializeObject<JObject>(response.Content)
                        ?.Property(nameof(ScalarResponse<string>.Item), StringComparison.OrdinalIgnoreCase)
                        ?.Value
                        .ToString();
                    Assert.NotNull(token);
                    var tokenPartsCount = token
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .SelectMany(part => part.Split('.', StringSplitOptions.RemoveEmptyEntries))
                        .Count();
                    Assert.Equal(3, tokenPartsCount);
                })
            };

            yield return new object?[]
            {
                new RestRequest($"http://127.0.0.1:5000/api/Auth/{nameof(AuthController.AuthenticateUser)}", Method.Get)
                    .AddHeader("Authorization", $"Bearer {tokenProvider.GenerateToken(username, TimeSpan.FromMinutes(1))}"),
                new Action<RestResponse, ITestOutputHelper>(static (response, output) =>
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.NotNull(response.Content);
                    output.WriteLine(response.Content);

                    var token = JsonConvert
                        .DeserializeObject<JObject>(response.Content)
                        ?.Property(nameof(ScalarResponse<string>.Item), StringComparison.OrdinalIgnoreCase)
                        ?.Value
                        .ToString();
                    Assert.NotNull(token);
                    var tokenPartsCount = token
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .SelectMany(part => part.Split('.', StringSplitOptions.RemoveEmptyEntries))
                        .Count();
                    Assert.Equal(3, tokenPartsCount);
                })
            };

            yield return new object?[]
            {
                new RestRequest($"http://127.0.0.1:5000/Test/{nameof(TestController.ApplicationInfo)}", Method.Get),
                new Action<RestResponse, ITestOutputHelper>(static (response, output) =>
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.NotNull(response.Content);
                    output.WriteLine(response.Content);
                })
            };

            yield return new object?[]
            {
                new RestRequest($"http://127.0.0.1:5000/Test/{nameof(TestController.Username)}", Method.Get)
                    .AddHeader("Authorization", $"Basic {(username, password).EncodeBasicAuth()}"),
                new Action<RestResponse, ITestOutputHelper>(static (response, output) =>
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.NotNull(response.Content);
                    output.WriteLine(response.Content);
                })
            };

            yield return new object?[]
            {
                new RestRequest($"http://127.0.0.1:5000/Test/{nameof(TestController.FakePost)}/List", Method.Get)
                    .AddHeader("Authorization", $"Bearer {tokenProvider.GenerateToken(username, TimeSpan.FromMinutes(1))}"),
                new Action<RestResponse, ITestOutputHelper>(static (response, output) =>
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.NotNull(response.Content);
                    output.WriteLine(response.Content);
                })
            };
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(WebControllerTestData))]
        internal async Task WebControllerTest(
            Lazy<IHost> host,
            CancellationTokenSource cts,
            AsyncCountdownEvent asyncCountdownEvent,
            RestRequest request,
            Action<RestResponse, ITestOutputHelper> assert)
        {
            try
            {
                Output.WriteLine(request.Resource);

                var hostShutdown = host.Value.WaitForShutdownAsync(cts.Token);

                using (var client = new RestClient())
                {
                    request.AddHeader("Cache-Control", "no-cache");

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

                    assert(response, Output);
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