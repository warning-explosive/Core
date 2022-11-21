namespace SpaceEngineers.Core.GenericHost.Test
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using AuthEndpoint.Contract;
    using Basics;
    using JwtAuthentication;
    using Microsoft.Extensions.Configuration;
    using SpaceEngineers.Core.Test.Api;
    using SpaceEngineers.Core.Test.Api.ClassFixtures;
    using Web.Auth;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// AuthTest
    /// </summary>
    [SuppressMessage("Analysis", "CA1506", Justification = "application composition root")]
    public class AuthTest : TestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">TestFixture</param>
        public AuthTest(ITestOutputHelper output, TestFixture fixture)
            : base(output, fixture)
        {
        }

        [Fact]
        internal void JwtTokenProviderTest()
        {
            var username = "qwerty";

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

            var token = tokenProvider.GenerateToken(username, TimeSpan.FromMinutes(5));
            Output.WriteLine(token);

            Assert.Equal(username, tokenProvider.GetUsername(token));
        }

        [Fact]
        internal void BasicAuthTest()
        {
            var username = "qwerty";
            var password = "12345678";

            var token = (username, password).EncodeBasicAuth();
            var credentials = token.DecodeBasicAuth();

            Output.WriteLine(token);

            Assert.Equal(username, credentials.username);
            Assert.Equal(password, credentials.password);
        }
    }
}