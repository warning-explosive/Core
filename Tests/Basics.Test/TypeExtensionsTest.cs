namespace SpaceEngineers.Core.Basics.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Basics;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// TypeExtensions class tests
    /// </summary>
    [SuppressMessage("Analysis", "SA1201", Justification = "For test reasons")]
    public class TypeExtensionsTest : BasicsTestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        public TypeExtensionsTest(ITestOutputHelper output)
            : base(output)
        {
        }

        [Theory]
        [InlineData(typeof(object), false)]
        [InlineData(typeof(string), false)]
        [InlineData(typeof(int), false)]
        [InlineData(typeof(KeyValuePair<string, object>), false)]
        [InlineData(typeof(TestRecord), true)]
        internal void IsRecordTest(Type type, bool result)
        {
            Assert.Equal(result, type.IsRecord());
        }

        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(SpaceEngineers.Core.Basics.Primitives.AsyncManualResetEvent))]
        [InlineData(typeof(SpaceEngineers.Core.Basics.Primitives.PriorityQueue<,>))]
        [InlineData(typeof(SpaceEngineers.Core.Basics.Primitives.PriorityQueue<string, string>))]
        [InlineData(typeof(SpaceEngineers.Core.Basics.Primitives.PriorityQueue<ICollection<string>, string>))]
        [InlineData(typeof(SpaceEngineers.Core.Basics.Primitives.PriorityQueue<Dictionary<string, ICollection<string>>, string>))]
        [InlineData(typeof(SpaceEngineers.Core.Basics.Primitives.PriorityQueue<Dictionary<string, string[]>, string>))]
        internal void FindTypeByFullNameTest(Type type)
        {
            var typeNode = TypeNode.FromType(type);

            Output.WriteLine(typeNode.ToString());

            typeNode = TypeNode.FromString(typeNode.ToString());

            var builtType = TypeNode.ToType(typeNode);

            Assert.Equal(type, builtType);
        }

        [Fact]
        internal void OrderByDependencyCycleDependencyTest()
        {
            var test1 = new[]
                       {
                           typeof(OrderByDependencyTestData.CycleDependencyTest1),
                           typeof(OrderByDependencyTestData.CycleDependencyTest2),
                           typeof(OrderByDependencyTestData.CycleDependencyTest3)
                       };

            Assert.Throws<InvalidOperationException>(() => test1.OrderByDependencies().ToArray());

            var test2 = new[]
                       {
                           typeof(OrderByDependencyTestData.CycleDependencyTest1),
                           typeof(OrderByDependencyTestData.CycleDependencyTest2),
                           typeof(OrderByDependencyTestData.CycleDependencyTest3),
                           typeof(OrderByDependencyTestData.CycleDependencyTest4)
                       };

            Assert.Throws<InvalidOperationException>(() => test2.OrderByDependencies().ToArray());
        }

        [Fact]
        internal void OrderByDependencyTest()
        {
            var test1 = new[]
            {
                typeof(OrderByDependencyTestData.DependencyTest1),
                typeof(OrderByDependencyTestData.DependencyTest2),
                typeof(OrderByDependencyTestData.DependencyTest3),
                typeof(OrderByDependencyTestData.DependencyTest4)
            };

            Assert.True(test1.Reverse().SequenceEqual(test1.OrderByDependencies()));

            var test2 = new[]
            {
                typeof(OrderByDependencyTestData.DependencyTest1),
                typeof(OrderByDependencyTestData.DependencyTest2),
                typeof(OrderByDependencyTestData.DependencyTest3),
                typeof(OrderByDependencyTestData.DependencyTest4)
            };

            Assert.True(test2.Reverse().SequenceEqual(test2.OrderByDependencies()));

            var test3 = new[]
            {
                typeof(OrderByDependencyTestData.GenericDependencyTest1<>),
                typeof(OrderByDependencyTestData.GenericDependencyTest2<>),
                typeof(OrderByDependencyTestData.GenericDependencyTest3<>),
                typeof(OrderByDependencyTestData.GenericDependencyTest4<>)
            };

            Assert.True(test3.Reverse().SequenceEqual(test3.OrderByDependencies()));

            var test4 = new[]
            {
                typeof(OrderByDependencyTestData.GenericDependencyTest1<object>),
                typeof(OrderByDependencyTestData.GenericDependencyTest2<string>),
                typeof(OrderByDependencyTestData.GenericDependencyTest3<int>),
                typeof(OrderByDependencyTestData.GenericDependencyTest4<bool>)
            };

            Assert.True(test4.Reverse().SequenceEqual(test4.OrderByDependencies()));
        }

        [Fact]
        internal void IsNullableTest()
        {
            Assert.False(typeof(bool).IsNullable());
            Assert.True(typeof(bool?).IsNullable());
            Assert.True(typeof(Nullable<>).IsNullable());

            Assert.False(typeof(string).IsNullable());
            Assert.False(typeof(object).IsNullable());

            var nullableString = (string?)string.Empty;
            var nullableObject = (object?)string.Empty;

            Assert.False(nullableString.GetType().IsNullable());
            Assert.False(nullableObject.GetType().IsNullable());
        }

        [Fact]
        internal void IsNullableMemberTest()
        {
            var notNullableValueField = typeof(ClassWithNullableMembers).GetField(nameof(ClassWithNullableMembers._notNullableValue));
            var nullableValueField = typeof(ClassWithNullableMembers).GetField(nameof(ClassWithNullableMembers._nullableValue));
            var notNullableReferenceField = typeof(ClassWithNullableMembers).GetField(nameof(ClassWithNullableMembers._notNullableReference));
            var nullableReferenceField = typeof(ClassWithNullableMembers).GetField(nameof(ClassWithNullableMembers._nullableReference));

            Assert.NotNull(notNullableValueField);
            Assert.NotNull(nullableValueField);
            Assert.NotNull(notNullableReferenceField);
            Assert.NotNull(nullableReferenceField);

            Assert.False(notNullableValueField!.IsNullable());
            Assert.True(nullableValueField!.IsNullable());
            Assert.False(notNullableReferenceField!.IsNullable());
            Assert.True(nullableReferenceField!.IsNullable());

            var notNullableValueProperty = typeof(ClassWithNullableMembers).GetProperty(nameof(ClassWithNullableMembers.NotNullableValue));
            var nullableValueProperty = typeof(ClassWithNullableMembers).GetProperty(nameof(ClassWithNullableMembers.NullableValue));
            var notNullableReferenceProperty = typeof(ClassWithNullableMembers).GetProperty(nameof(ClassWithNullableMembers.NotNullableReference));
            var nullableReferenceProperty = typeof(ClassWithNullableMembers).GetProperty(nameof(ClassWithNullableMembers.NullableReference));

            Assert.NotNull(notNullableValueProperty);
            Assert.NotNull(nullableValueProperty);
            Assert.NotNull(notNullableReferenceProperty);
            Assert.NotNull(nullableReferenceProperty);

            Assert.False(notNullableValueProperty!.IsNullable());
            Assert.True(nullableValueProperty!.IsNullable());
            Assert.False(notNullableReferenceProperty!.IsNullable());
            Assert.True(nullableReferenceProperty!.IsNullable());
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
            Assert.True(typeof(ITestGenericInterfaceBase<>).IsSubclassOfOpenGeneric(typeof(ITestGenericInterfaceBase<>)));
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
        internal void ExtractGenericArgumentsAtTest()
        {
            Assert.Equal(typeof(string), typeof(ITestGenericInterface<string>).ExtractGenericArgumentsAt(typeof(ITestGenericInterfaceBase<>), 0).Single());
            Assert.Equal(typeof(object), typeof(TestTypeImplementation).ExtractGenericArgumentsAt(typeof(ITestGenericInterfaceBase<>), 0).Single());
            Assert.Equal(typeof(object), typeof(DirectTestTypeImplementation).ExtractGenericArgumentsAt(typeof(ITestGenericInterfaceBase<>), 0).Single());
            Assert.Equal(typeof(bool), typeof(TestGenericTypeImplementation<bool>).ExtractGenericArgumentsAt(typeof(TestGenericTypeImplementationBase<>), 0).Single());

            Assert.Equal(typeof(bool), typeof(ClosedImplementation).ExtractGenericArgumentsAt(typeof(ITestInterface<,>), 0).Single());
            Assert.Equal(typeof(object), typeof(ClosedImplementation).ExtractGenericArgumentsAt(typeof(ITestInterface<,>), 1).Single());
            Assert.Equal("T1", typeof(OpenedImplementation<,>).ExtractGenericArgumentsAt(typeof(ITestInterface<,>), 0).Single().Name);
            Assert.Equal("T2", typeof(OpenedImplementation<,>).ExtractGenericArgumentsAt(typeof(ITestInterface<,>), 1).Single().Name);
            Assert.Equal("T1", typeof(HalfOpenedImplementation<>).ExtractGenericArgumentsAt(typeof(ITestInterface<,>), 0).Single().Name);
            Assert.Equal(typeof(object), typeof(HalfOpenedImplementation<>).ExtractGenericArgumentsAt(typeof(ITestInterface<,>), 1).Single());
            Assert.True(new List<Type> { typeof(bool), typeof(string) }.SequenceEqual(typeof(SeveralImplementations).ExtractGenericArgumentsAt(typeof(ITestInterface<,>), 0).ToList()));
            Assert.True(new List<Type> { typeof(object), typeof(int) }.SequenceEqual(typeof(SeveralImplementations).ExtractGenericArgumentsAt(typeof(ITestInterface<,>), 1).ToList()));

            Assert.Throws<ArgumentException>(() => typeof(ITestGenericInterface<string>).ExtractGenericArgumentsAt(typeof(ITestGenericInterfaceBase<bool>), 0).Any());
            Assert.Throws<ArgumentException>(() => typeof(TestTypeImplementation).ExtractGenericArgumentsAt(typeof(ITestGenericInterfaceBase<bool>), 0).Any());
            Assert.Throws<ArgumentException>(() => typeof(DirectTestTypeImplementation).ExtractGenericArgumentsAt(typeof(ITestGenericInterfaceBase<bool>), 0).Any());
            Assert.Throws<ArgumentException>(() => typeof(TestGenericTypeImplementation<bool>).ExtractGenericArgumentsAt(typeof(TestGenericTypeImplementationBase<bool>), 0).Any());
        }

        [Fact]
        internal void FitsForTypeArgumentTest()
        {
            Assert.True(typeof(bool).FitsForTypeArgument(typeof(ITestInterface<,>).GetGenericArguments()[0]));
            Assert.True(typeof(Enum).FitsForTypeArgument(typeof(ITestInterface<,>).GetGenericArguments()[0]));
            Assert.True(typeof(StringSplitOptions).FitsForTypeArgument(typeof(ITestInterface<,>).GetGenericArguments()[0]));
            Assert.True(typeof(StructWithParameter).FitsForTypeArgument(typeof(ITestInterface<,>).GetGenericArguments()[0]));
            Assert.True(typeof(object).FitsForTypeArgument(typeof(ITestInterface<,>).GetGenericArguments()[0]));
            Assert.True(typeof(ClassWithParameter).FitsForTypeArgument(typeof(ITestInterface<,>).GetGenericArguments()[0]));

            Assert.True(typeof(bool).FitsForTypeArgument(typeof(ITestInterface<,>).GetGenericArguments()[1]));
            Assert.True(typeof(Enum).FitsForTypeArgument(typeof(ITestInterface<,>).GetGenericArguments()[1]));
            Assert.True(typeof(StringSplitOptions).FitsForTypeArgument(typeof(ITestInterface<,>).GetGenericArguments()[1]));
            Assert.True(typeof(StructWithParameter).FitsForTypeArgument(typeof(ITestInterface<,>).GetGenericArguments()[1]));
            Assert.True(typeof(object).FitsForTypeArgument(typeof(ITestInterface<,>).GetGenericArguments()[1]));
            Assert.True(typeof(ClassWithParameter).FitsForTypeArgument(typeof(ITestInterface<,>).GetGenericArguments()[1]));

            Assert.False(typeof(bool).FitsForTypeArgument(typeof(IClassConstrained<>).GetGenericArguments()[0]));
            Assert.True(typeof(Enum).FitsForTypeArgument(typeof(IClassConstrained<>).GetGenericArguments()[0]));
            Assert.False(typeof(StringSplitOptions).FitsForTypeArgument(typeof(IClassConstrained<>).GetGenericArguments()[0]));
            Assert.False(typeof(StructWithParameter).FitsForTypeArgument(typeof(IClassConstrained<>).GetGenericArguments()[0]));
            Assert.True(typeof(object).FitsForTypeArgument(typeof(IClassConstrained<>).GetGenericArguments()[0]));
            Assert.True(typeof(ClassWithParameter).FitsForTypeArgument(typeof(IClassConstrained<>).GetGenericArguments()[0]));

            Assert.True(typeof(bool).FitsForTypeArgument(typeof(IStructConstrained<>).GetGenericArguments()[0]));
            Assert.False(typeof(Enum).FitsForTypeArgument(typeof(IStructConstrained<>).GetGenericArguments()[0]));
            Assert.True(typeof(StringSplitOptions).FitsForTypeArgument(typeof(IStructConstrained<>).GetGenericArguments()[0]));
            Assert.True(typeof(StructWithParameter).FitsForTypeArgument(typeof(IStructConstrained<>).GetGenericArguments()[0]));
            Assert.False(typeof(object).FitsForTypeArgument(typeof(IStructConstrained<>).GetGenericArguments()[0]));
            Assert.False(typeof(ClassWithParameter).FitsForTypeArgument(typeof(IStructConstrained<>).GetGenericArguments()[0]));

            Assert.True(typeof(bool).FitsForTypeArgument(typeof(IDefaultCtorConstrained<>).GetGenericArguments()[0]));
            Assert.False(typeof(Enum).FitsForTypeArgument(typeof(IDefaultCtorConstrained<>).GetGenericArguments()[0]));
            Assert.True(typeof(StringSplitOptions).FitsForTypeArgument(typeof(IDefaultCtorConstrained<>).GetGenericArguments()[0]));
            Assert.True(typeof(StructWithParameter).FitsForTypeArgument(typeof(IDefaultCtorConstrained<>).GetGenericArguments()[0]));
            Assert.True(typeof(object).FitsForTypeArgument(typeof(IDefaultCtorConstrained<>).GetGenericArguments()[0]));
            Assert.False(typeof(ClassWithParameter).FitsForTypeArgument(typeof(IDefaultCtorConstrained<>).GetGenericArguments()[0]));

            Assert.True(typeof(HalfOpenedImplementation<Guid>).FitsForTypeArgument(typeof(ITestInterface<Guid, object>)));
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

        private interface IClassConstrained<T>
            where T : class { }

        private interface IStructConstrained<T>
            where T : struct { }

        private interface IDefaultCtorConstrained<T>
            where T : new() { }

        [SuppressMessage("Analysis", "CA1801", Justification = "For test reasons")]
        private class ClassWithParameter
        {
            public ClassWithParameter(object param) { }
        }

        [SuppressMessage("Analysis", "CA1801", Justification = "For test reasons")]
        private struct StructWithParameter
        {
            public StructWithParameter(object param) { }
        }

        [SuppressMessage("Analysis", "SA1401", Justification = "For test reasons")]
        private class ClassWithNullableMembers
        {
            public bool _notNullableValue;

            public bool? _nullableValue;

            public object _notNullableReference = null!;

            public object? _nullableReference;

            public bool NotNullableValue
            {
                get => _notNullableValue;
                set => _notNullableValue = value;
            }

            public bool? NullableValue
            {
                get => _nullableValue;
                set => _nullableValue = value;
            }

            public object NotNullableReference
            {
                get => _notNullableReference;
                set => _notNullableReference = value;
            }

            public object? NullableReference
            {
                get => _nullableReference;
                set => _nullableReference = value;
            }
        }
    }
}