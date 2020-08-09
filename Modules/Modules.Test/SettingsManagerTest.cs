namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;
    using Basics;
    using SettingsManager.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// SettingsManager class tests
    /// </summary>
    public class SettingsManagerTest : ModulesTestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        public SettingsManagerTest(ITestOutputHelper output)
            : base(output) { }

        internal interface ITestSettings
        {
            int Int { get; set; }

            decimal Decimal { get; set; }

            string? String { get; set; }

            DateTime Date { get; set; }
        }

        /// <summary> Data member </summary>
        /// <returns>Test data</returns>
        public static IEnumerable<object[]> Data()
        {
            yield return new object[] { typeof(TestYamlSettings), new Func<object>(TestYamlSettings.CreateYamlSettings) };
            yield return new object[] { typeof(TestJsonSettings), new Func<object>(TestJsonSettings.CreateJsonSettings) };
        }

        [Theory]
        [MemberData(nameof(Data))]
        internal async Task ReadWriteTest(Type cfgType, Func<object> cfgFactory)
        {
            await this.CallMethod(nameof(ReadWriteTestInternal))
                      .WithTypeArgument(cfgType)
                      .WithArgument(cfgFactory)
                      .Invoke<Task>()
                      .ConfigureAwait(false);
        }

        internal async Task ReadWriteTestInternal<TSettings>(Func<object> cfgFactory)
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

        internal class TestYamlSettings : ITestSettings, IYamlSettings
        {
            public TestYamlSettings()
            {
                var date = DateTime.MinValue;

                Int = date.Year;
                Decimal = 42.42m;
                String = "Hello world!";
                Date = date;
            }

            public int Int { get; set; }

            public decimal Decimal { get; set; }

            public string? String { get; set; }

            public DateTime Date { get; set; }

            public TestYamlSettings? Reference { get; set; }

            public ICollection<TestYamlSettings>? Collection { get; set; }

            public IDictionary<string, TestYamlSettings>? Dictionary { get; set; }

            internal static TestYamlSettings CreateYamlSettings()
            {
                var inner = new TestYamlSettings();

                return new TestYamlSettings
                       {
                           Reference = inner,
                           Collection = new List<TestYamlSettings> { inner },
                           Dictionary = new Dictionary<string, TestYamlSettings> { ["First"] = inner }
                       };
            }
        }

        internal class TestJsonSettings : ITestSettings, IJsonSettings
        {
            public TestJsonSettings(ITestSettings reference)
            {
                var date = DateTime.MinValue;

                Int = date.Year;
                Decimal = 42.42m;
                String = "Hello world!";
                Date = date;

                Reference = reference;
                Collection = new List<ITestSettings> { reference };
                Dictionary = new Dictionary<string, ITestSettings> { ["First"] = reference };
            }

            public int Int { get; set; }

            public decimal Decimal { get; set; }

            public string? String { get; set; }

            public DateTime Date { get; set; }

            public ITestSettings? Reference { get; set; }

            public ICollection<ITestSettings>? Collection { get; set; }

            public IDictionary<string, ITestSettings>? Dictionary { get; set; }

            internal static TestJsonSettings CreateJsonSettings()
            {
                var inner = new TestYamlSettings();

                return new TestJsonSettings(inner);
            }
        }
    }
}