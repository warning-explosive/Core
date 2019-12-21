namespace SpaceEngineers.Core.Basics.Test
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Basics;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// TypeExtensions class tests
    /// </summary>
    [SuppressMessage("StyleCop.Analyzers", "SA1201", Justification = "For test reasons")]
    public class TypeExtensionsTest : BasicsTestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        public TypeExtensionsTest(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        internal void OrderByDependencyCycleDependencyTest()
        {
            var test = new[]
                       {
                           typeof(OrderByDependencyTestData.CycleDependencyTest1),
                           typeof(OrderByDependencyTestData.CycleDependencyTest2),
                           typeof(OrderByDependencyTestData.CycleDependencyTest3),
                       };

            Assert.Throws<InvalidOperationException>(() => test.OrderByDependencies(z => z).ToArray());
        }

        [Fact]
        internal void OrderByDependencyTest()
        {
            var test1 = new[]
            {
                typeof(OrderByDependencyTestData.DependencyTest1),
                typeof(OrderByDependencyTestData.DependencyTest2),
                typeof(OrderByDependencyTestData.DependencyTest3),
            };

            Assert.True(test1.Reverse().SequenceEqual(test1.OrderByDependencies(z => z)));

            var test2 = new[]
            {
                typeof(OrderByDependencyTestData.GenericDependencyTest1<>),
                typeof(OrderByDependencyTestData.GenericDependencyTest2<>),
                typeof(OrderByDependencyTestData.GenericDependencyTest3<>),
            };

            Assert.True(test2.Reverse().SequenceEqual(test2.OrderByDependencies(z => z)));

            var test3 = new[]
            {
                typeof(OrderByDependencyTestData.GenericDependencyTest1<object>),
                typeof(OrderByDependencyTestData.GenericDependencyTest2<string>),
                typeof(OrderByDependencyTestData.GenericDependencyTest3<int>),
            };

            Assert.True(test3.Reverse().SequenceEqual(test3.OrderByDependencies(z => z)));
        }

        [Fact]
        internal void IsImplementationOfOpenGenericTest()
        {
            Assert.True(typeof(TestTypeImplementation).IsImplementationOfOpenGeneric(typeof(TestGenericTypeImplementationBase<>)));
            Assert.True(typeof(TestGenericTypeImplementation<object>).IsImplementationOfOpenGeneric(typeof(TestGenericTypeImplementationBase<>)));
            Assert.True(typeof(TestGenericTypeImplementation<object>).IsImplementationOfOpenGeneric(typeof(TestGenericTypeImplementation<>)));

            Assert.False(typeof(TestTypeImplementation).IsImplementationOfOpenGeneric(typeof(TestGenericTypeImplementationBase<object>)));
            Assert.False(typeof(TestTypeImplementation).IsImplementationOfOpenGeneric(typeof(ITestGenericInterface<>)));
            Assert.False(typeof(TestTypeImplementation).IsImplementationOfOpenGeneric(typeof(ITestGenericInterface<object>)));
        }

        [Fact]
        internal void IsImplementationOfOpenGenericInterfaceTest()
        {
            Assert.True(typeof(TestTypeImplementation).IsImplementationOfOpenGenericInterface(typeof(ITestGenericInterface<>)));
            Assert.True(typeof(TestTypeImplementation).IsImplementationOfOpenGenericInterface(typeof(ITestGenericInterfaceBase<>)));

            Assert.False(typeof(TestTypeImplementation).IsImplementationOfOpenGenericInterface(typeof(ITestGenericInterface<object>)));
            Assert.False(typeof(TestTypeImplementation).IsImplementationOfOpenGenericInterface(typeof(ITestGenericInterfaceBase<object>)));
            Assert.False(typeof(TestTypeImplementation).IsImplementationOfOpenGenericInterface(typeof(ITestInterface)));

            Assert.False(typeof(TestTypeImplementation).IsImplementationOfOpenGenericInterface(typeof(TestGenericTypeImplementationBase<>)));
            Assert.False(typeof(TestTypeImplementation).IsImplementationOfOpenGenericInterface(typeof(TestGenericTypeImplementationBase<object>)));
        }

        [Fact]
        internal void IsDerivedFromInterfaceTest()
        {
            Assert.True(typeof(TestTypeImplementation).IsDerivedFromInterface(typeof(ITestGenericInterface<object>)));
            Assert.True(typeof(TestTypeImplementation).IsDerivedFromInterface(typeof(ITestGenericInterfaceBase<object>)));
            Assert.True(typeof(TestTypeImplementation).IsDerivedFromInterface(typeof(ITestInterface)));

            Assert.True(typeof(TestGenericTypeImplementation<object>).IsDerivedFromInterface(typeof(ITestGenericInterface<object>)));

            Assert.False(typeof(TestTypeImplementation).IsDerivedFromInterface(typeof(ITestGenericInterface<>)));
            Assert.False(typeof(TestTypeImplementation).IsDerivedFromInterface(typeof(TestGenericTypeImplementationBase<>)));
        }

        [Fact]
        internal void IsContainsInterfaceDeclarationTest()
        {
            Assert.True(typeof(ITestGenericInterfaceBase<object>).IsContainsInterfaceDeclaration(typeof(ITestInterface)));
            Assert.False(typeof(ITestGenericInterface<object>).IsContainsInterfaceDeclaration(typeof(ITestInterface)));

            Assert.False(typeof(ITestInterface).IsContainsInterfaceDeclaration(typeof(ITestInterface)));
            Assert.False(typeof(TestTypeImplementation).IsContainsInterfaceDeclaration(typeof(ITestInterface)));

            Assert.False(typeof(TestGenericTypeImplementationBase<object>).IsContainsInterfaceDeclaration(typeof(ITestGenericInterface<string>)));
            Assert.False(typeof(ITestGenericInterface<object>).IsContainsInterfaceDeclaration(typeof(ITestGenericInterfaceBase<string>)));

            Assert.True(typeof(TestGenericTypeImplementationBase<object>).IsContainsInterfaceDeclaration(typeof(ITestGenericInterface<object>)));
            Assert.True(typeof(ITestGenericInterface<object>).IsContainsInterfaceDeclaration(typeof(ITestGenericInterfaceBase<object>)));
        }

        private interface ITestInterface { }

        private interface ITestGenericInterfaceBase<T> : ITestInterface { }

        private interface ITestGenericInterface<T> : ITestGenericInterfaceBase<T>, ITestInterface { }

        private abstract class TestGenericTypeImplementationBase<T> : ITestGenericInterface<T> { }

        private class TestTypeImplementation : TestGenericTypeImplementationBase<object> { }

        private class TestGenericTypeImplementation<T> : TestGenericTypeImplementationBase<T> { }
    }
}