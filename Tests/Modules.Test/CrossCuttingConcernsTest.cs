namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using CrossCuttingConcerns.ObjectBuilder;
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
            var assembly = AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.CrossCuttingConcerns)));

            var options = new DependencyContainerOptions();

            DependencyContainer = fixture.BoundedAboveContainer(output, options, assembly);
        }

        private IDependencyContainer DependencyContainer { get; }

        [Fact]
        internal void ObjectBuilderTest()
        {
            Assert.NotNull(DependencyContainer.Resolve<IObjectBuilder<object>>().Build());

            var objectValues = new Dictionary<string, object?> { ["value"] = new object() };
            Assert.Throws<InvalidOperationException>(() => DependencyContainer.Resolve<IObjectBuilder<object>>().Build(objectValues));

            var str = "qwerty";
            var stringValues = new Dictionary<string, object?> { ["value"] = str };
            Assert.Equal(str, DependencyContainer.Resolve<IObjectBuilder<string>>().Build(stringValues));

            var enumValues = new Dictionary<string, object?> { ["value"] = EnLifestyle.Scoped.ToString() };
            Assert.Equal(EnLifestyle.Scoped, DependencyContainer.Resolve<IObjectBuilder<EnLifestyle>>().Build(enumValues));

            var guid = Guid.NewGuid();
            var guidValues = new Dictionary<string, object?> { ["value"] = guid };
            Assert.Equal(guid, DependencyContainer.Resolve<IObjectBuilder<Guid>>().Build(guidValues));

            Assert.Equal(string.Empty, DependencyContainer.Resolve<IObjectBuilder<TestClass>>().Build().Value);
            var testStructValues = new Dictionary<string, object?> { [nameof(TestClass.Value)] = str };
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