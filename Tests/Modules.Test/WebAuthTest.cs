namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Text;
    using AuthorizationEndpoint.JwtAuthentication;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// WebAuth assembly tests
    /// </summary>
    public class WebAuthTest : TestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">ModulesTestFixture</param>
        public WebAuthTest(ITestOutputHelper output, ModulesTestFixture fixture)
            : base(output, fixture)
        {
        }

        [Fact]
        internal void JwtTokenProviderTest()
        {
            var userName = "qwerty";

            var issuer = "Test";
            var audience = "Test";

            // var privateKey = Convert.ToBase64String(new HMACSHA256().Key)
            var privateKey = "db3OIsj+BXE9NZDy0t8W3TcNekrF+2d/1sFnWG4HnV8TZY30iTOdtVWJG8abWvB1GlOgJuQZdcF2Luqm/hccMw==";

            var configuration = new JwtAuthenticationConfiguration(issuer, audience, privateKey);
            var tokenProvider = new JwtTokenProvider(configuration);

            var token = tokenProvider.GenerateToken(userName, TimeSpan.FromMinutes(5));
            Output.WriteLine(token);

            Assert.Equal(userName, tokenProvider.GetUsername(token));
        }

        [Fact]
        internal void BasicAuthTest()
        {
            var userName = "qwerty";
            var password = "1234";

            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userName}:{password}"));
            Output.WriteLine(token);

            var credentialBytes = Convert.FromBase64String(token);
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);

            Assert.Equal(userName, credentials[0]);
            Assert.Equal(password, credentials[1]);
        }
    }
}