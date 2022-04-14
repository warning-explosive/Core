namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using CompositionRoot.Api.Abstractions;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using CrossCuttingConcerns.Settings;
    using Registrations;
    using Settings;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// ISettingsProvider class tests
    /// </summary>
    public class SettingsProviderTest : TestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">ModulesTestFixture</param>
        public SettingsProviderTest(ITestOutputHelper output, ModulesTestFixture fixture)
            : base(output, fixture)
        {
            DependencyContainer = fixture.ModulesContainer(output);
        }

        private IDependencyContainer DependencyContainer { get; }

        /// <summary> ReadWriteTest data member </summary>
        /// <returns>Test data</returns>
        public static IEnumerable<object[]> ReadWriteTestData()
        {
            yield return new object[]
            {
                typeof(FileSystemSettings),
                new Action(() => { }),
                new Action<FileSystemSettings>(settings =>
                {
                    Assert.NotNull(settings.FileSystemSettingsDirectory);
                    Assert.Equal(Path.Combine(SolutionExtensions.ProjectFile().Directory.FullName, "Settings"), settings.FileSystemSettingsDirectory);
                })
            };
            yield return new object[]
            {
                typeof(TestEnvironmentSettings),
                new Action(() =>
                {
                    Environment.SetEnvironmentVariable(
                        nameof(TestEnvironmentSettings),
                        $@"{{ ""{nameof(TestEnvironmentSettings.Int)}"": 42, ""{nameof(TestEnvironmentSettings.Decimal)}"": 42.42, ""{nameof(TestEnvironmentSettings.String)}"": ""Hello world!"", ""{nameof(TestEnvironmentSettings.Date)}"": ""{new DateTime(2000, 1, 1):yyyy-MM-ddTHH:mm:ss}"" }}",
                        EnvironmentVariableTarget.Process);
                }),
                new Action<TestEnvironmentSettings>(settings =>
                {
                    Assert.Equal(42, settings.Int);
                    Assert.Equal(42.42m, settings.Decimal);
                    Assert.Equal("Hello world!", settings.String);
                    Assert.Equal(new DateTime(2000, 1, 1), settings.Date);
                })
            };
            yield return new object[]
            {
                typeof(TestYamlSettings),
                new Action(() => { }),
                new Action<TestYamlSettings>(settings =>
                {
                    AssertPrimitives(settings);

                    Assert.NotNull(settings.Reference);
                    AssertCircularReference(settings.Reference!);

                    Assert.NotNull(settings.Collection);
                    Assert.Single(settings.Collection!);
                    AssertCircularReference(settings.Collection!.Single());

                    Assert.NotNull(settings.Dictionary);
                    Assert.Single(settings.Dictionary!);
                    Assert.Equal("First", settings.Dictionary!.Single().Key);
                    AssertCircularReference(settings.Dictionary!.Single().Value);

                    static void AssertPrimitives(TestYamlSettings settings)
                    {
                        Assert.Equal(42, settings.Int);
                        Assert.Equal(42.42m, settings.Decimal);
                        Assert.Equal("Hello world!", settings.String);
                        Assert.Equal(new DateTime(2000, 1, 1), settings.Date);
                    }

                    static void AssertCircularReference(TestYamlSettings settings)
                    {
                        AssertPrimitives(settings);
                        Assert.Null(settings.Reference);
                        Assert.Null(settings.Collection);
                        Assert.Null(settings.Dictionary);
                    }
                })
            };
            yield return new object[]
            {
                typeof(TestJsonSettings),
                new Action(() => { }),
                new Action<TestJsonSettings>(settings =>
                {
                    AssertPrimitives(settings);

                    Assert.NotNull(settings.Reference);
                    AssertCircularReference(settings.Reference!);

                    Assert.NotNull(settings.Collection);
                    Assert.Single(settings.Collection!);
                    AssertCircularReference(settings.Collection!.Single());

                    Assert.NotNull(settings.Dictionary);
                    Assert.Single(settings.Dictionary!);
                    Assert.Equal("First", settings.Dictionary!.Single().Key);
                    AssertCircularReference(settings.Dictionary!.Single().Value);

                    static void AssertPrimitives(TestJsonSettings settings)
                    {
                        Assert.Equal(42, settings.Int);
                        Assert.Equal(42.42m, settings.Decimal);
                        Assert.Equal("Hello world!", settings.String);
                        Assert.Equal(new DateTime(2000, 1, 1), settings.Date);
                    }

                    static void AssertCircularReference(TestJsonSettings settings)
                    {
                        AssertPrimitives(settings);
                        Assert.Null(settings.Reference);
                        Assert.Null(settings.Collection);
                        Assert.Null(settings.Dictionary);
                    }
                })
            };
            yield return new object[]
            {
                typeof(TestConfigurationSettings),
                new Action(() => { }),
                new Action<TestConfigurationSettings>(settings =>
                {
                    AssertPrimitives(settings);

                    Assert.NotNull(settings.Reference);
                    AssertCircularReference(settings.Reference!);

                    Assert.NotNull(settings.Collection);
                    Assert.Single(settings.Collection!);
                    AssertCircularReference(settings.Collection!.Single());

                    Assert.NotNull(settings.Dictionary);
                    Assert.Single(settings.Dictionary!);
                    Assert.Equal("First", settings.Dictionary!.Single().Key);
                    AssertCircularReference(settings.Dictionary!.Single().Value);

                    static void AssertPrimitives(TestConfigurationSettings settings)
                    {
                        Assert.Equal(42, settings.Int);
                        Assert.Equal(42.42m, settings.Decimal);
                        Assert.Equal("Hello world!", settings.String);
                        Assert.Equal(new DateTime(2000, 1, 1), settings.Date);
                    }

                    static void AssertCircularReference(TestConfigurationSettings settings)
                    {
                        AssertPrimitives(settings);
                        Assert.Null(settings.Reference);
                        Assert.Null(settings.Collection);
                        Assert.Null(settings.Dictionary);
                    }
                })
            };
            yield return new object[]
            {
                typeof(PersistenceSettings),
                new Action(() => { }),
                new Action<PersistenceSettings>(settings =>
                {
                    Assert.NotNull(settings.MongoClientSettings);
                    Assert.Equal(TimeSpan.FromSeconds(30), settings.MongoClientSettings.ConnectTimeout);
                    Assert.Equal(TimeSpan.FromSeconds(10), settings.MongoClientSettings.HeartbeatInterval);
                    Assert.Equal(100, settings.MongoClientSettings.MaxConnectionPoolSize);
                    Assert.Equal(0, settings.MongoClientSettings.MinConnectionPoolSize);
                    Assert.Single(settings.MongoClientSettings.Servers);
                    Assert.Equal("localhost", settings.MongoClientSettings.Servers.Single().Host);
                    Assert.Equal(27017, settings.MongoClientSettings.Servers.Single().Port);
                    Assert.Equal(TimeSpan.FromSeconds(30), settings.MongoClientSettings.ServerSelectionTimeout);
                })
            };
        }

        [Theory]
        [MemberData(nameof(ReadWriteTestData))]
        internal async Task ReadWriteTest(Type cfgType, object arrange, object assert)
        {
            Output.WriteLine(cfgType.Name);

            var fileSystemSettings = await DependencyContainer
               .Resolve<ISettingsProvider<FileSystemSettings>>()
               .Get(CancellationToken.None)
               .ConfigureAwait(false);

            Output.WriteLine(fileSystemSettings.FileSystemSettingsDirectory);

            await this
               .CallMethod(nameof(ReadWriteTestInternal))
               .WithTypeArgument(cfgType)
               .WithArguments(arrange, assert)
               .Invoke<Task>()
               .ConfigureAwait(false);
        }

        private async Task ReadWriteTestInternal<TSettings>(
            Action arrange,
            Action<TSettings> assert)
            where TSettings : class, ISettings, new()
        {
            arrange();

            var settings = await DependencyContainer
               .Resolve<ISettingsProvider<TSettings>>()
               .Get(CancellationToken.None)
               .ConfigureAwait(false);

            Assert.NotNull(settings);

            Output.WriteLine(settings.ShowProperties(BindingFlags.Instance | BindingFlags.Public));
            Output.WriteLine(string.Empty);

            assert(settings);
        }
    }
}