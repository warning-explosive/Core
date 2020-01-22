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
        internal void IsSubclassOfOpenGenericTest()
        {
            // ITestInterface
            Assert.False(typeof(ITestGenericInterfaceBase<>).IsSubclassOfOpenGeneric(typeof(ITestInterface)));
            Assert.False(typeof(TestTypeImplementation).IsSubclassOfOpenGeneric(typeof(ITestInterface)));
            Assert.False(typeof(ITestGenericInterface<>).IsSubclassOfOpenGeneric(typeof(ITestInterface)));
            Assert.False(typeof(TestGenericTypeImplementationBase<>).IsSubclassOfOpenGeneric(typeof(ITestInterface)));

            // ITestGenericInterfaceBase<T>
            Assert.False(typeof(ITestGenericInterfaceBase<>).IsSubclassOfOpenGeneric(typeof(ITestGenericInterfaceBase<>)));
            Assert.True(typeof(ITestGenericInterface<>).IsSubclassOfOpenGeneric(typeof(ITestGenericInterfaceBase<>)));
            Assert.True(typeof(TestGenericTypeImplementationBase<>).IsSubclassOfOpenGeneric(typeof(ITestGenericInterfaceBase<>)));
            Assert.True(typeof(TestTypeImplementation).IsSubclassOfOpenGeneric(typeof(ITestGenericInterfaceBase<>)));
            Assert.True(typeof(TestGenericTypeImplementation<>).IsSubclassOfOpenGeneric(typeof(ITestGenericInterfaceBase<>)));

            // ITestGenericInterface<object>
            Assert.False(typeof(ITestGenericInterface<>).IsSubclassOfOpenGeneric(typeof(ITestGenericInterface<object>)));
            Assert.False(typeof(TestGenericTypeImplementationBase<>).IsSubclassOfOpenGeneric(typeof(ITestGenericInterface<object>)));
            Assert.False(typeof(TestTypeImplementation).IsSubclassOfOpenGeneric(typeof(ITestGenericInterface<object>)));
            Assert.False(typeof(TestGenericTypeImplementation<>).IsSubclassOfOpenGeneric(typeof(ITestGenericInterface<object>)));
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