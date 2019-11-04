namespace SpaceEngineers.Core.Utilities.Test
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using CompositionRoot;
    using CompositionRoot.Extensions;
    using CompositionRoot.Test;
    using SettingsManager;
    using Xunit;
    using Xunit.Abstractions;
    
    public class SettingsManagerTest : TestBase
    {
        public SettingsManagerTest(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void YamlDeserializationTest()
        {
            var manager = DependencyContainer.Resolve<ISettingsManger>();

            /*
             * 1 - Read
             */
            var config = manager.Get<TestYamlConfig>();
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
            config = manager.Get<TestYamlConfig>();
            Assert.NotNull(config);
            Output.WriteLine(config.ShowProperties(BindingFlags.Instance | BindingFlags.Public));
        }
    }

    public class TestYamlConfig : IFileSystemSettings<IYamlFormatter>
    {
        public int Int { get; set; }
        
        public decimal Decimal { get; set; }
        
        public string? String { get; set; }

        public DateTime Date { get; set; }

        public InnerTestConfig? Reference { get; set; }

        public ICollection<InnerTestConfig>? Collection { get; set; }
    }

    public class InnerTestConfig
    {
        public int Int { get; set; }
        
        public decimal Decimal { get; set; }
        
        public string? String { get; set; }

        public DateTime Date { get; set; }
    }
}