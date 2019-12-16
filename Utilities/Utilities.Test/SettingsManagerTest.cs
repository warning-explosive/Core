namespace SpaceEngineers.Core.Utilities.Test
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Basics;
    using CompositionRoot.Test;
    using SettingsManager;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// SettingsManager class tests
    /// </summary>
    public class SettingsManagerTest : CompositionRootTestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        public SettingsManagerTest(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        internal void YamlDeserializationTest()
        {
            var manager = DependencyContainer.Resolve<ISettingsManager<TestYamlConfig>>();

            /*
             * 1 - Read
             */
            var config = manager.Get();
            Assert.NotNull(config);
            Output.WriteLine(config.ShowProperties(BindingFlags.Instance | BindingFlags.Public));
            Output.WriteLine(string.Empty);

            /*
             * 2 - Write
             */
            var date = DateTime.Now;

            config = new TestYamlConfig
                     {
                         Int = date.Year,
                         Decimal = 42.42m,
                         String = "Hello world!",
                         Date = date,
                         Reference =
                             new InnerTestConfig
                             {
                                 Int = date.Year,
                                 Decimal = 42.42m,
                                 String = "Hello world!",
                                 Date = date
                             },
                         Collection = new List<InnerTestConfig>
                                      {
                                          new InnerTestConfig
                                          {
                                              Int = date.Year,
                                              Decimal = 42.42m,
                                              String = "Hello world!",
                                              Date = date
                                          }
                                      }
                     };
            manager.Set(config);
            Output.WriteLine(config.ShowProperties(BindingFlags.Instance | BindingFlags.Public));
            Output.WriteLine(string.Empty);

            /*
             * 3 - Read again
             */
            config = manager.Get();
            Assert.NotNull(config);
            Output.WriteLine(config.ShowProperties(BindingFlags.Instance | BindingFlags.Public));
        }

        private class TestYamlConfig : IYamlSettings
        {
            public int Int { get; set; }

            public decimal Decimal { get; set; }

            public string? String { get; set; }

            public DateTime Date { get; set; }

            public InnerTestConfig? Reference { get; set; }

            public ICollection<InnerTestConfig>? Collection { get; set; }
        }

        private class InnerTestConfig
        {
            public int Int { get; set; }

            public decimal Decimal { get; set; }

            public string? String { get; set; }

            public DateTime Date { get; set; }
        }
    }
}