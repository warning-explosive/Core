namespace SpaceEngineers.Core.Modules.Test
{
    using AutoWiringApi.Attributes;
    using AutoWiringTest;
    using Basics;
    using Basics.Test;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// AutoWiringApi assembly tests
    /// </summary>
    public class AutoWiringApiTest : BasicsTestBase
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