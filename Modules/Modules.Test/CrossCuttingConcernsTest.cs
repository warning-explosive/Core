namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Enumerations;
    using Basics;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using CrossCuttingConcerns.Api.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// CrossCuttingConcerns assembly tests
    /// </summary>
    public class CrossCuttingConcernsTest : TestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">ModulesTestFixture</param>
        public CrossCuttingConcernsTest(ITestOutputHelper output, ModulesTestFixture fixture)
            : base(output, fixture)
        {
            var assembly = AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.CrossCuttingConcerns)));

            var options = new DependencyContainerOptions();

            DependencyContainer = fixture.BoundedAboveContainer(options, assembly);
        }

        /// <summary>
        /// DependencyContainer
        /// </summary>
        protected IDependencyContainer DependencyContainer { get; }

        [Fact]
        internal void ObjectBuilderTest()
        {
            Assert.NotNull(DependencyContainer.Resolve<IObjectBuilder<object>>().Build());

            var objectValues = new Dictionary<string, object> { ["value"] = new object() };
            Assert.Throws<InvalidOperationException>(() => DependencyContainer.Resolve<IObjectBuilder<object>>().Build(objectValues));

            var str = "qwerty";
            var stringValues = new Dictionary<string, object> { ["value"] = str };
            Assert.Equal(str, DependencyContainer.Resolve<IObjectBuilder<string>>().Build(stringValues));

            var enumValues = new Dictionary<string, object> { ["value"] = EnComponentRegistrationKind.AutomaticallyRegistered.ToString() };
            Assert.Equal(EnComponentRegistrationKind.AutomaticallyRegistered, DependencyContainer.Resolve<IObjectBuilder<EnComponentRegistrationKind>>().Build(enumValues));

            var guid = Guid.NewGuid();
            var guidValues = new Dictionary<string, object> { ["value"] = guid };
            Assert.Equal(guid, DependencyContainer.Resolve<IObjectBuilder<Guid>>().Build(guidValues));

            Assert.Equal(string.Empty, DependencyContainer.Resolve<IObjectBuilder<TestClass>>().Build().Value);
            var testStructValues = new Dictionary<string, object> { [nameof(TestClass.Value)] = str };
            Assert.Equal(str, DependencyContainer.Resolve<IObjectBuilder<TestClass>>().Build(testStructValues).Value);
        }

        private class TestClass
        {
            public TestClass()
            {
                Value = string.Empty;
            }

            public TestClass(char[] value)
            {
                Value = new string(value);
            }

            internal string Value { get; }
        }
    }
}