namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;
    using AutoRegistration.Abstractions;
    using Basics;
    using Basics.Test;
    using ClassFixtures;
    using Core.SettingsManager.Abstractions;
    using GenericEndpoint.Abstractions;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Settings;
    using GenericHost;
    using Registrations;
    using Settings;
    using SettingsManager;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// SettingsManager class tests
    /// </summary>
    public class SettingsManagerTest : BasicsTestBase, IClassFixture<ModulesTestFixture>
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">ModulesTestFixture</param>
        public SettingsManagerTest(ITestOutputHelper output, ModulesTestFixture fixture)
            : base(output)
        {
            var excludedAssemblies = new[]
            {
                typeof(IIntegrationMessage).Assembly, // GenericEndpoint.Contract
                typeof(IGenericEndpoint).Assembly, // GenericEndpoint
                typeof(GenericHost).Assembly // GenericHost
            };

            var registrations = new IManualRegistration[]
            {
                new DelegatesRegistration(),
                new VersionedOpenGenericRegistration()
            };

            DependencyContainer = fixture.GetDependencyContainer(typeof(SettingsManagerTest).Assembly, excludedAssemblies, registrations);
        }

        /// <summary>
        /// DependencyContainer
        /// </summary>
        protected IDependencyContainer DependencyContainer { get; }

        /// <summary> ReadWriteTest data member </summary>
        /// <returns>Test data</returns>
        public static IEnumerable<object[]> ReadWriteTestData()
        {
            yield return new object[] { typeof(TestYamlSettings), new Func<object>(TestYamlSettings.CreateYamlSettings) };
            yield return new object[] { typeof(TestJsonSettings), new Func<object>(TestJsonSettings.CreateJsonSettings) };
        }

        [Fact]
        internal async Task EnvironmentSettingsSetTest()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(Set)
                        .ConfigureAwait(false);

            Task Set()
            {
                return DependencyContainer.Resolve<ISettingsManager<EnvironmentSettings>>()
                                          .Set(new EnvironmentSettings(new List<EnvironmentSettingsEntry>()));
            }
        }

        [Fact]
        internal async Task EnvironmentSettingsGetTest()
        {
            var entryKey = nameof(EnvironmentSettingsGetTest);

            var before = await Get().ConfigureAwait(false);

            Assert.DoesNotContain(before.Settings, entry => entry.Key == entryKey);

            Environment.SetEnvironmentVariable(entryKey, entryKey, EnvironmentVariableTarget.Process);

            var after = await Get().ConfigureAwait(false);

            Assert.Contains(after.Settings, entry => entry.Key == entryKey);

            Task<EnvironmentSettings> Get()
            {
                return DependencyContainer.Resolve<ISettingsManager<EnvironmentSettings>>().Get();
            }
        }

        [Theory]
        [MemberData(nameof(ReadWriteTestData))]
        internal async Task ReadWriteTest(Type cfgType, Func<object> cfgFactory)
        {
            await this.CallMethod(nameof(ReadWriteTestInternal))
                      .WithTypeArgument(cfgType)
                      .WithArgument(cfgFactory)
                      .Invoke<Task>()
                      .ConfigureAwait(false);
        }

        [Theory]
        [InlineData(typeof(QueueConventions))]
        [InlineData(typeof(PersistenceSettings))]
        [InlineData(typeof(TransportSettings))]
        internal async Task GenericEndpointSettingsReadWriteTest(Type settingsType)
        {
            await this.CallMethod(nameof(GenericEndpointSettingsReadWriteTestInternal))
                .WithTypeArgument(settingsType)
                .Invoke<Task>()
                .ConfigureAwait(false);
        }

        private async Task ReadWriteTestInternal<TSettings>(Func<object> cfgFactory)
            where TSettings : class, ISettings
        {
            var manager = DependencyContainer.Resolve<ISettingsManager<TSettings>>();

            /*
             * 1 - Read
             */
            var config = await manager.Get().ConfigureAwait(false);
            Assert.NotNull(config);
            Output.WriteLine(config.ShowProperties(BindingFlags.Instance | BindingFlags.Public));
            Output.WriteLine(string.Empty);

            /*
             * 2 - Write
             */
            config = (TSettings)cfgFactory();

            await manager.Set(config).ConfigureAwait(false);
            Output.WriteLine(config.ShowProperties(BindingFlags.Instance | BindingFlags.Public));
            Output.WriteLine(string.Empty);

            /*
             * 3 - Read again
             */
            config = await manager.Get().ConfigureAwait(false);
            Assert.NotNull(config);
            Output.WriteLine(config.ShowProperties(BindingFlags.Instance | BindingFlags.Public));
        }

        private async Task GenericEndpointSettingsReadWriteTestInternal<TSetting>()
            where TSetting : class, IJsonSettings
        {
            var manager = DependencyContainer.Resolve<ISettingsManager<TSetting>>();

            var setting = await manager.Get().ConfigureAwait(false);
            Assert.NotNull(setting);

            await manager.Set(setting).ConfigureAwait(false);
        }
    }
}