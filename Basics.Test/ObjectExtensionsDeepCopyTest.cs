namespace SpaceEngineers.Core.Basics.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using Basics;
    using DeepCopy;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// DeepCopy test
    /// </summary>
    [SuppressMessage("StyleCop.Analyzers", "SA1201", Justification = "For test reasons")]
    public class ObjectExtensionsDeepCopyTest : BasicsTestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        public ObjectExtensionsDeepCopyTest(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        internal void DeepCopyObjectTest()
        {
            var original = new object();
            var clone = original.DeepCopy();

            Assert.NotEqual(original, clone);
            Assert.False(ReferenceEquals(original, clone));

            var instance1 = new object();
            var instance2 = new object();
            clone = instance1.DeepCopy();

            Assert.True(instance1.GetType() == instance2.GetType());
            Assert.True(ReferenceEquals(instance1.GetType(), instance2.GetType()));

            Assert.True(instance1.GetType() == clone.GetType());
            Assert.True(ReferenceEquals(instance1.GetType(), clone.GetType()));
        }

        [Fact]
        internal void DeepCopyTest()
        {
            var original = TestReferenceWithSystemTypes.Create();
            var clone = original.DeepCopy();

            AssertTestReferenceTypeWithTypes(original, clone, false);
        }

        [Fact]
        internal void DeepCopyBySerializationThrowsTest()
        {
            var original = TestReferenceWithSystemTypes.Create();

            Assert.Throws<SerializationException>(() => original.DeepCopyBySerialization());
        }

        [Fact]
        internal void DeepCopyBySerializationTest()
        {
            var original = TestReferenceWithoutSystemTypes.CreateOrInit();
            var clone = original.DeepCopyBySerialization();

            AssertTestReferenceTypeWithOutTypes(original, clone, true);
        }

        private static void AssertTestReferenceTypeWithTypes(TestReferenceWithSystemTypes original,
                                                             TestReferenceWithSystemTypes clone,
                                                             bool bySerialization)
        {
            /*
             * Type
             */
            Assert.True(original.GetType() == clone.GetType());
            Assert.True(ReferenceEquals(original.GetType(), clone.GetType()));
            Assert.True(original.Type == clone.Type);
            Assert.True(ReferenceEquals(original.Type, clone.Type));

            Assert.False(original.TypeArray?.Equals(clone.TypeArray));
            Assert.False(ReferenceEquals(original.TypeArray, clone.TypeArray));
            Assert.True(CheckReferenceArraySequentially<Type>(original.TypeArray, clone.TypeArray));

            Assert.False(original.TypeCollection?.Equals(clone.TypeCollection));
            Assert.False(ReferenceEquals(original.TypeCollection, clone.TypeCollection));
            Assert.True(original.TypeCollection?.SequenceEqual(clone.TypeCollection));

            AssertTestReferenceTypeWithOutTypes(original, clone, bySerialization);
        }

        private static void AssertTestReferenceTypeWithOutTypes(TestReferenceWithoutSystemTypes original,
                                                                TestReferenceWithoutSystemTypes clone,
                                                                bool bySerialization)
        {
            /*
             * String
             */
            Assert.Equal(original.String, clone.String);
            Assert.True(bySerialization
                            ? !ReferenceEquals(original.String, clone.String)
                            : ReferenceEquals(original.String, clone.String));

            /*
             * ValueType
             */
            Assert.Equal(original.Int, clone.Int);

            Assert.Equal(original.TestEnum, clone.TestEnum);

            Assert.False(original.ValueTypeArray?.Equals(clone.ValueTypeArray));
            Assert.False(ReferenceEquals(original.ValueTypeArray, clone.ValueTypeArray));
            Assert.True(CheckValueArraySequentially<int>(original.ValueTypeArray, clone.ValueTypeArray));

            Assert.False(original.ValueTypeCollection?.Equals(clone.ValueTypeCollection));
            Assert.False(ReferenceEquals(original.ValueTypeCollection, clone.ValueTypeCollection));
            Assert.True(original.ValueTypeCollection?.SequenceEqual(clone.ValueTypeCollection));

            /*
             * ReferenceType
             */
            Assert.False(original.ReferenceTypeArray?.Equals(clone.ReferenceTypeArray));
            Assert.False(ReferenceEquals(original.ReferenceTypeArray, clone.ReferenceTypeArray));
            Assert.False(CheckReferenceArraySequentially<object>(original.ReferenceTypeArray, clone.ReferenceTypeArray));

            Assert.False(original.ReferenceTypeCollection?.Equals(clone.ReferenceTypeCollection));
            Assert.False(ReferenceEquals(original.ReferenceTypeCollection, clone.ReferenceTypeCollection));
            Assert.False(original.ReferenceTypeCollection?.SequenceEqual(clone.ReferenceTypeCollection));

            Assert.True(original.Equals(original.CyclicReference));
            Assert.True(ReferenceEquals(original, original.CyclicReference));
            Assert.True(clone.Equals(clone.CyclicReference));
            Assert.True(ReferenceEquals(clone, clone.CyclicReference));

            Assert.False(original.Equals(clone));
            Assert.False(ReferenceEquals(original, clone));

            Assert.False(original.Equals(clone.CyclicReference));
            Assert.False(ReferenceEquals(original, clone.CyclicReference));

            Assert.False(original.CyclicReference?.Equals(clone.CyclicReference));
            Assert.False(ReferenceEquals(original.CyclicReference, clone.CyclicReference));

            Assert.True(original.Equals(TestReferenceWithoutSystemTypes.StaticCyclicReference));
            Assert.True(ReferenceEquals(original, TestReferenceWithoutSystemTypes.StaticCyclicReference));
            Assert.False(clone.Equals(TestReferenceWithoutSystemTypes.StaticCyclicReference));
            Assert.False(ReferenceEquals(clone, TestReferenceWithoutSystemTypes.StaticCyclicReference));

            /*
             * Nullable
             */
            Assert.Null(clone.NullableInt);
            Assert.Null(clone.NullableReference);
            Assert.True(clone.CollectionOfNulls?.All(z => z == null));
            Assert.True(CheckArray(clone.ArrayOfNulls, null));
        }

        private static bool CheckArray(Array? array, object? value)
        {
            if (array == null)
            {
                return false;
            }

            foreach (var item in array)
            {
                if (item != value)
                {
                    return false;
                }
            }

            return true;
        }

        [SuppressMessage("Microsoft.CodeQuality.Analyzers", "CA1508", Justification = "Analyzer error")]
        private static bool CheckValueArraySequentially<T>(Array? array, Array? compareWith)
            where T : struct
        {
            if (array == null || compareWith == null)
            {
                return false;
            }

            for (var i = 0; i < array.Length; ++i)
            {
                var left = (T?)array.GetValue(i);
                var right = (T?)compareWith.GetValue(i);

                if (left == null
                    || right == null
                    || !((T)left).Equals((T)right))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool CheckReferenceArraySequentially<T>(Array? array, Array? compareWith)
            where T : class
        {
            if (array == null || compareWith == null)
            {
                return false;
            }

            for (var i = 0; i < array.Length; ++i)
            {
                var left = (T?)array.GetValue(i);
                var right = (T?)compareWith.GetValue(i);

                if (left == null
                    || right == null
                    || !left.Equals(right))
                {
                    return false;
                }
            }

            return true;
        }
    }
}