namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Linq;
    using System.Reflection;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using CrossCuttingConcerns.Settings;
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
            var projectFileDirectory = SolutionExtensions.ProjectFile().Directory
                                       ?? throw new InvalidOperationException("Project directory not found");

            var settingsDirectory = projectFileDirectory
                .StepInto("Settings")
                .StepInto(nameof(ReadSettingsTest));

            var assemblies = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(CrossCuttingConcerns)))
            };

            var additionalOurTypes = new[]
            {
                typeof(TestConfigurationSettings)
            };

            var options = new DependencyContainerOptions()
                .WithAdditionalOurTypes(additionalOurTypes)
                .WithManualRegistrations(new SettingsDirectoryProviderManualRegistration(new SettingsDirectoryProvider(settingsDirectory)))
                .WithManualRegistrations(Fixture.DelegateRegistration(container =>
                {
                    container.Register<ISettingsProvider<TestConfigurationSettings>, AppSettingsJsonSettingsProvider<TestConfigurationSettings>>(EnLifestyle.Singleton);
                }));

            DependencyContainer = Fixture.BoundedAboveContainer(Output, options, assemblies);
        }

        private IDependencyContainer DependencyContainer { get; }

        [Fact]
        internal void ReadSettingsTest()
        {
            var settings = DependencyContainer
                .Resolve<ISettingsProvider<TestConfigurationSettings>>()
                .Get();

            Assert.NotNull(settings);

            Output.WriteLine(settings.ShowProperties(BindingFlags.Instance | BindingFlags.Public));
            Output.WriteLine(string.Empty);

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
        }
    }
}