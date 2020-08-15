namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Abstractions;
    using AutoRegistration.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using AutoWiringTest;
    using Basics;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// AutoWiringApi assembly tests
    /// </summary>
    public class AutoWiringApiTest : ModulesTestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        public AutoWiringApiTest(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        internal void ManualRegistrationAttributeTest()
        {
            Assert.True(typeof(RegisteredByDelegateImpl).HasAttribute<ManualRegistrationAttribute>());
            Assert.True(typeof(ConcreteRegisteredByDelegate).HasAttribute<ManualRegistrationAttribute>());

            Assert.NotNull(typeof(RegisteredByDelegateImpl).GetAttribute<ManualRegistrationAttribute>());
            Assert.NotNull(typeof(ConcreteRegisteredByDelegate).GetAttribute<ManualRegistrationAttribute>());
        }
    }
}