namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using CrossCuttingConcerns.Settings;
    using Newtonsoft.Json;
    using Overrides;
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
        /// <param name="fixture">TestFixture</param>
        public SettingsProviderTest(ITestOutputHelper output, TestFixture fixture)
            : base(output, fixture)
        {
            var assemblies = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(CrossCuttingConcerns))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(MongoDB), nameof(MongoDB.Driver)))
            };

            var additionalOurTypes = new[]
            {
                typeof(TestConfigurationSettings),
                typeof(TestEnvironmentSettings),
                typeof(TestJsonSettings),
                typeof(TestYamlSettings),
                typeof(TestPersistenceSettings)
            };

            var options = new DependencyContainerOptions()
               .WithAdditionalOurTypes(additionalOurTypes)
               .WithManualRegistrations(fixture.DelegateRegistration(container =>
               {
                   container.Register<ISettingsProvider<TestConfigurationSettings>, TestConfigurationSettingsProvider>(EnLifestyle.Singleton);
                   container.Register<ISettingsProvider<TestEnvironmentSettings>, TestEnvironmentSettingsProvider>(EnLifestyle.Singleton);
                   container.Register<ISettingsProvider<TestJsonSettings>, TestJsonSettingsProvider>(EnLifestyle.Singleton);
                   container.Register<ISettingsProvider<TestYamlSettings>, TestYamlSettingsProvider>(EnLifestyle.Singleton);
                   container.Register<ISettingsProvider<TestPersistenceSettings>, TestPersistenceSettingsProvider>(EnLifestyle.Singleton);
                   container.RegisterCollectionEntry<JsonConverter, MongoServerAddressJsonConverter>(EnLifestyle.Singleton);
               }))
               .WithOverrides(new TestSettingsScopeProviderOverride(nameof(ReadSettingsTest)));

            DependencyContainer = fixture.BoundedAboveContainer(output, options, assemblies);
        }

        private IDependencyContainer DependencyContainer { get; }

        /// <summary> ReadSettingsTest data member </summary>
        /// <returns>Test data</returns>
        public static IEnumerable<object[]> ReadSettingsTestData()
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
                typeof(TestPersistenceSettings),
                new Action(() => { }),
                new Action<TestPersistenceSettings>(settings =>
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
        [MemberData(nameof(ReadSettingsTestData))]
        internal async Task ReadSettingsTest(Type settingsType, object arrange, object assert)
        {
            SolutionExtensions
               .ProjectFile()
               .Directory
               .EnsureNotNull("Project directory not found")
               .StepInto("Settings")
               .SetupFileSystemSettingsDirectory();

            Output.WriteLine(settingsType.Name);

            var fileSystemSettings = await DependencyContainer
               .Resolve<ISettingsProvider<FileSystemSettings>>()
               .Get(CancellationToken.None)
               .ConfigureAwait(false);

            Output.WriteLine(fileSystemSettings.FileSystemSettingsDirectory);

            await this
               .CallMethod(nameof(GenericReadSettingsTest))
               .WithTypeArgument(settingsType)
               .WithArguments(arrange, assert)
               .Invoke<Task>()
               .ConfigureAwait(false);
        }

        private async Task GenericReadSettingsTest<TSettings>(
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