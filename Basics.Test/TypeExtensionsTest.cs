namespace SpaceEngineers.Core.Basics.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
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
            Assert.False(typeof(DirectTestTypeImplementation).IsContainsInterfaceDeclaration(typeof(ITestGenericInterface<string>)));

            Assert.True(typeof(TestGenericTypeImplementationBase<object>).IsContainsInterfaceDeclaration(typeof(ITestGenericInterface<object>)));
            Assert.True(typeof(ITestGenericInterface<object>).IsContainsInterfaceDeclaration(typeof(ITestGenericInterfaceBase<object>)));
            Assert.True(typeof(DirectTestTypeImplementation).IsContainsInterfaceDeclaration(typeof(ITestGenericInterface<object>)));

            Assert.False(typeof(TestGenericTypeImplementationBase<object>).IsContainsInterfaceDeclaration(typeof(ITestGenericInterface<>)));
            Assert.False(typeof(ITestGenericInterface<object>).IsContainsInterfaceDeclaration(typeof(ITestGenericInterfaceBase<>)));
            Assert.False(typeof(DirectTestTypeImplementation).IsContainsInterfaceDeclaration(typeof(ITestGenericInterface<>)));
        }

        [Fact]
        internal void GetGenericArgumentsOfOpenGenericAtTest()
        {
            Assert.Equal(typeof(string), typeof(ITestGenericInterface<string>).GetGenericArgumentsOfOpenGenericAt(typeof(ITestGenericInterfaceBase<>), 0).Single());
            Assert.Equal(typeof(object), typeof(TestTypeImplementation).GetGenericArgumentsOfOpenGenericAt(typeof(ITestGenericInterfaceBase<>), 0).Single());
            Assert.Equal(typeof(object), typeof(DirectTestTypeImplementation).GetGenericArgumentsOfOpenGenericAt(typeof(ITestGenericInterfaceBase<>), 0).Single());
            Assert.Equal(typeof(bool), typeof(TestGenericTypeImplementation<bool>).GetGenericArgumentsOfOpenGenericAt(typeof(TestGenericTypeImplementationBase<>), 0).Single());

            Assert.Equal(typeof(bool), typeof(ClosedImplementation).GetGenericArgumentsOfOpenGenericAt(typeof(ITestInterface<,>), 0).Single());
            Assert.Equal(typeof(object), typeof(ClosedImplementation).GetGenericArgumentsOfOpenGenericAt(typeof(ITestInterface<,>), 1).Single());
            Assert.Equal("T1", typeof(OpenedImplementation<,>).GetGenericArgumentsOfOpenGenericAt(typeof(ITestInterface<,>), 0).Single().Name);
            Assert.Equal("T2", typeof(OpenedImplementation<,>).GetGenericArgumentsOfOpenGenericAt(typeof(ITestInterface<,>), 1).Single().Name);
            Assert.Equal("T1", typeof(HalfOpenedImplementation<>).GetGenericArgumentsOfOpenGenericAt(typeof(ITestInterface<,>), 0).Single().Name);
            Assert.Equal(typeof(object), typeof(HalfOpenedImplementation<>).GetGenericArgumentsOfOpenGenericAt(typeof(ITestInterface<,>), 1).Single());
            Assert.True(new List<Type> { typeof(bool), typeof(string) }.SequenceEqual(typeof(SeveralImplementations).GetGenericArgumentsOfOpenGenericAt(typeof(ITestInterface<,>), 0).ToList()));
            Assert.True(new List<Type> { typeof(object), typeof(int) }.SequenceEqual(typeof(SeveralImplementations).GetGenericArgumentsOfOpenGenericAt(typeof(ITestInterface<,>), 1).ToList()));

            Assert.Throws<ArgumentException>(() => typeof(ITestGenericInterface<string>).GetGenericArgumentsOfOpenGenericAt(typeof(ITestGenericInterfaceBase<bool>), 0).Any());
            Assert.Throws<ArgumentException>(() => typeof(TestTypeImplementation).GetGenericArgumentsOfOpenGenericAt(typeof(ITestGenericInterfaceBase<bool>), 0).Any());
            Assert.Throws<ArgumentException>(() => typeof(DirectTestTypeImplementation).GetGenericArgumentsOfOpenGenericAt(typeof(ITestGenericInterfaceBase<bool>), 0).Any());
            Assert.Throws<ArgumentException>(() => typeof(TestGenericTypeImplementation<bool>).GetGenericArgumentsOfOpenGenericAt(typeof(TestGenericTypeImplementationBase<bool>), 0).Any());
        }

        private interface ITestInterface { }

        private interface ITestInterface<T1, T2> { }

        private interface ITestGenericInterfaceBase<T> : ITestInterface { }

        private interface ITestGenericInterface<T> : ITestGenericInterfaceBase<T>, ITestInterface { }

        private abstract class TestGenericTypeImplementationBase<T> : ITestGenericInterface<T> { }

        private class DirectTestTypeImplementation : ITestGenericInterface<object> { }

        private class TestTypeImplementation : TestGenericTypeImplementationBase<object> { }

        private class TestGenericTypeImplementation<T> : TestGenericTypeImplementationBase<T> { }

        private class ClosedImplementation : ITestInterface<bool, object> { }

        private class OpenedImplementation<T1, T2> : ITestInterface<T1, T2> { }

        private class HalfOpenedImplementation<T1> : ITestInterface<T1, object> { }

        private class SeveralImplementations : ITestInterface<bool, object>, ITestInterface<string, int> { }
    }
}