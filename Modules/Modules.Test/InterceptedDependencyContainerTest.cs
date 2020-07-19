namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// IInterceptedContainer class test
    /// </summary>
    public class InterceptedDependencyContainerTest : ModulesTestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        public InterceptedDependencyContainerTest(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        internal void SimpleResolveTest()
        {
            throw new NotImplementedException();
        }

        [Fact]
        internal void ResolveCollectionTest()
        {
            throw new NotImplementedException();
        }

        [Fact]
        internal void DecoratorAsDependencyTest()
        {
            throw new NotImplementedException();
        }

        [Fact]
        internal void SeveralDependenciesTest()
        {
            throw new NotImplementedException();
        }

        [Fact]
        internal void NestedScopesTest()
        {
            throw new NotImplementedException();
        }

        [Fact]
        internal void ApplyAfterRegisteredDecoratorsTest()
        {
            throw new NotImplementedException();
        }

        [Fact]
        internal void RegisterAndApplyDecoratorTest()
        {
            throw new NotImplementedException();
        }
    }
}