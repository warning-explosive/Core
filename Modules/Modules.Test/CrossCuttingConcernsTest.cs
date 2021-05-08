namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using CrossCuttingConcerns;
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
            var assembly = typeof(CrossCuttingConcernsManualRegistration).Assembly; // CrossCuttingConcerns

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
            var stringValues = new Dictionary<string, object> { ["value"] = string.Empty };
            Assert.NotNull(DependencyContainer.Resolve<IObjectBuilder<string>>().Build(stringValues));

            Assert.Equal(string.Empty, DependencyContainer.Resolve<IObjectBuilder<TestClass>>().Build().Value);
            var testStructValues = new Dictionary<string, object> { [nameof(TestClass.Value)] = string.Empty };
            Assert.Equal(string.Empty, DependencyContainer.Resolve<IObjectBuilder<TestClass>>().Build(testStructValues).Value);
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