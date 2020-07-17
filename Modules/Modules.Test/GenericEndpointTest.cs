namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Threading.Tasks;
    using Basics;
    using GenericEndpoint.Settings;
    using SettingsManager.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// GenericEndpoint project test
    /// </summary>
    public class GenericEndpointTest : ModulesTestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        public GenericEndpointTest(ITestOutputHelper output)
            : base(output)
        {
        }

        [Theory]
        [InlineData(typeof(QueueConventions))]
        [InlineData(typeof(PersistenceSettings))]
        [InlineData(typeof(TransportSettings))]
        internal async Task GenericEndpointSettingsReadWriteTest(Type settingsType)
        {
            // TODO: Support constructors with parameters
            await this.CallMethod(nameof(GenericEndpointSettingsReadWriteTestInternal))
                      .WithTypeArgument(settingsType)
                      .Invoke<Task>()
                      .ConfigureAwait(false);
        }

        internal async Task GenericEndpointSettingsReadWriteTestInternal<TSetting>()
            where TSetting : class, IYamlSettings
        {
            var manager = DependencyContainer.Resolve<ISettingsManager<TSetting>>();

            var setting = await manager.Get().ConfigureAwait(false);
            Assert.NotNull(setting);

            await manager.Set(setting).ConfigureAwait(false);
        }
    }
}