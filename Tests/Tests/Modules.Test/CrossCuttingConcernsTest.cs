namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using CrossCuttingConcerns.ObjectBuilder;
    using CrossCuttingConcerns.Settings;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// CrossCuttingConcerns assembly tests
    /// </summary>
    public class CrossCuttingConcernsTest : TestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">TestFixture</param>
        public CrossCuttingConcernsTest(ITestOutputHelper output, TestFixture fixture)
            : base(output, fixture)
        {
            var projectFileDirectory = SolutionExtensions.ProjectFile().Directory
                                       ?? throw new InvalidOperationException("Project directory wasn't found");

            var settingsDirectory = projectFileDirectory.StepInto("Settings");

            var assemblies = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(CrossCuttingConcerns)))
            };

            var options = new DependencyContainerOptions()
                .WithPluginAssemblies(assemblies)
                .WithManualRegistrations(new SettingsDirectoryProviderManualRegistration(new SettingsDirectoryProvider(settingsDirectory)));

            DependencyContainer = fixture.DependencyContainer(options);
        }

        private IDependencyContainer DependencyContainer { get; }

        [Fact]
        internal void ObjectBuilderTest()
        {
            Assert.Equal(
                "qwerty",
                DependencyContainer.Resolve<IObjectBuilder<string>>().Build(new Dictionary<string, object?> { ["value"] = "qwerty" }));

            Assert.Equal(
                EnLifestyle.Scoped,
                DependencyContainer.Resolve<IObjectBuilder<EnLifestyle>>().Build(new Dictionary<string, object?> { ["value"] = EnLifestyle.Scoped.ToString() }));

            Assert.Equal(
                Guid.Parse("50EA09BF-C8C1-494B-8D35-8B4C86A5A344"),
                DependencyContainer.Resolve<IObjectBuilder<Guid>>().Build(new Dictionary<string, object?> { ["value"] = Guid.Parse("50EA09BF-C8C1-494B-8D35-8B4C86A5A344") }));

            Assert.Equal(
                TypeNode.FromType(TypeExtensions.FindType("System.Private.CoreLib System.DateOnly")),
                DependencyContainer.Resolve<IObjectBuilder<TypeNode>>().Build(new Dictionary<string, object?> { ["value"] = "System.Private.CoreLib System.DateOnly" }));

            Assert.Equal(
                TypeExtensions.FindType("System.Private.CoreLib System.DateOnly"),
                DependencyContainer.Resolve<IObjectBuilder<Type>>().Build(new Dictionary<string, object?> { ["value"] = "System.Private.CoreLib System.DateOnly" }));

            Assert.Equal(
                "System.Private.CoreLib System.DateOnly",
                DependencyContainer.Resolve<IObjectBuilder<string>>().Build(new Dictionary<string, object?> { ["value"] = TypeExtensions.FindType("System.Private.CoreLib System.DateOnly") }));

            Assert.Equal(
                "qwerty",
                DependencyContainer.Resolve<IObjectBuilder<TestClass>>().Build(new Dictionary<string, object?> { [nameof(TestClass.Value)] = "qwerty" }).Value);
        }

        private class TestClass
        {
            public TestClass(string value)
            {
                Value = value;
            }

            internal string Value { get; }
        }
    }
}