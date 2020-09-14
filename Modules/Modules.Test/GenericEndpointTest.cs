namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Threading.Tasks;
    using Basics;
    using Core.SettingsManager.Abstractions;
    using GenericEndpoint.Settings;
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
            await this.CallMethod(nameof(GenericEndpointSettingsReadWriteTestInternal))
                      .WithTypeArgument(settingsType)
                      .Invoke<Task>()
                      .ConfigureAwait(false);
        }

        internal async Task GenericEndpointSettingsReadWriteTestInternal<TSetting>()
            where TSetting : class, IJsonSettings
        {
            var manager = DependencyContainer.Resolve<ISettingsManager<TSetting>>();

            var setting = await manager.Get().ConfigureAwait(false);
            Assert.NotNull(setting);

            await manager.Set(setting).ConfigureAwait(false);
        }
    }
}