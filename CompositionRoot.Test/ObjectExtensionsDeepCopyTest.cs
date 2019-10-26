namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using Extensions;
    using Xunit;
    using Xunit.Abstractions;

    public class ObjectExtensionsDeepCopyTest : TestBase
    {
        public ObjectExtensionsDeepCopyTest(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public void DeepCopyPerfomanceTest()
        {
            var original = InitInstanceOfTestReferenceTypeWithOutTypes();

            var sw1 = new Stopwatch();
            var sw2 = new Stopwatch();

            void SingleBatch()
            {
                sw1.Start();
                var cloneBySerialization = original.DeepCopyBySerialization();
                sw1.Stop();
            
                Output.WriteLine("BySerialization");
                Output.WriteLine(sw1.ShowProperties(BindingFlags.Instance | BindingFlags.Public));
                Output.WriteLine("=======");
            
                sw2.Start();
                var cloneByReflection = original.DeepCopy();
                sw2.Stop();
            
                Output.WriteLine("ByReflection");
                Output.WriteLine(sw2.ShowProperties(BindingFlags.Instance | BindingFlags.Public));
                Output.WriteLine("=======");
            
                AssertTestReferenceTypeWithOutTypes(original, cloneBySerialization, true);
                AssertTestReferenceTypeWithOutTypes(original, cloneByReflection, false);
            
                Output.WriteLine($"Profit = {sw1.ElapsedTicks / sw2.ElapsedTicks}");
                Assert.True(sw1.ElapsedTicks > sw2.ElapsedTicks);
            }

            for (int i = 1; i <= 10; ++i)
            {
                SingleBatch();   
            }
        }

        [Fact]
        public void DeepCopyObjectTest()
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
        public void DeepCopyTest()
        {
            var original = InitInstanceOfTestReferenceTypeWithTypes();
            var clone = original.DeepCopy();
            
            AssertTestReferenceTypeWithTypes(original, clone, false);
        }
        
        [Fact]
        public void DeepCopyBySerializationThrowsTest()
        {
            var original = InitInstanceOfTestReferenceTypeWithTypes();
            
            Assert.Throws<SerializationException>(() => original.DeepCopyBySerialization());
        }
        
        [Fact]
        public void DeepCopyBySerializationTest()
        {
            var original = InitInstanceOfTestReferenceTypeWithOutTypes();
            var clone = original.DeepCopyBySerialization();
            
            AssertTestReferenceTypeWithOutTypes(original, clone, true);
        }

        private static void AssertTestReferenceTypeWithTypes(TestReferenceTypeWithTypes original,
                                                             TestReferenceTypeWithTypes clone,
                                                             bool bySerialization)
        {
            #region Type
            
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
            #endregion

            AssertTestReferenceTypeWithOutTypes(original, clone, bySerialization);
        }

        private static void AssertTestReferenceTypeWithOutTypes(TestReferenceTypeWithOutTypes original,
                                                                TestReferenceTypeWithOutTypes clone,
                                                                bool bySerialization)
        {
            #region String
            
            Assert.Equal(original.String, clone.String);
            Assert.True(bySerialization
                            ? !ReferenceEquals(original.String, clone.String)
                            : ReferenceEquals(original.String, clone.String));

            #endregion
            
            #region ValueType
            
            Assert.Equal(original.Int, clone.Int);
            
            Assert.Equal(original.TestEnum, clone.TestEnum);
            
            Assert.False(original.ValueTypeArray?.Equals(clone.ValueTypeArray));
            Assert.False(ReferenceEquals(original.ValueTypeArray, clone.ValueTypeArray));
            Assert.True(CheckValueArraySequentially<int>(original.ValueTypeArray, clone.ValueTypeArray));

            Assert.False(original.ValueTypeCollection?.Equals(clone.ValueTypeCollection));
            Assert.False(ReferenceEquals(original.ValueTypeCollection, clone.ValueTypeCollection));
            Assert.True(original.ValueTypeCollection?.SequenceEqual(clone.ValueTypeCollection));

            #endregion

            #region ReferenceType
            
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
            
            Assert.True(original.Equals(TestReferenceTypeWithOutTypes.StaticCyclicReference));
            Assert.True(ReferenceEquals(original, TestReferenceTypeWithOutTypes.StaticCyclicReference));
            Assert.False(clone.Equals(TestReferenceTypeWithOutTypes.StaticCyclicReference));
            Assert.False(ReferenceEquals(clone, TestReferenceTypeWithOutTypes.StaticCyclicReference));
            
            #endregion
            
            #region Nullable

            Assert.Null(clone.NullableInt);
            Assert.Null(clone.NullableReference);
            Assert.True(clone.CollectionOfNulls?.All(z => z == null));
            Assert.True(CheckArray(clone.ArrayOfNulls, null));

            #endregion
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
        
        private static bool CheckValueArraySequentially<T>(Array? array, Array? compareWith)
            where T : struct
        {
            if (array == null || compareWith == null)
            {
                return false;
            }

            for(var i = 0; i < array?.Length; ++i)
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

            for(var i = 0; i < array?.Length; ++i)
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

        private TestReferenceTypeWithTypes InitInstanceOfTestReferenceTypeWithTypes()
        {
            var instance = new TestReferenceTypeWithTypes
                {
                    Type = typeof(TestReferenceTypeWithOutTypes),
                    TypeArray = new[] { typeof(TestReferenceTypeWithOutTypes), typeof(string), typeof(int) },
                    TypeCollection = new List<Type> { typeof(TestReferenceTypeWithOutTypes), typeof(string), typeof(int) },
                };

            InitInstanceOfTestReferenceTypeWithOutTypes(instance);

            return instance;
        }

        private TestReferenceTypeWithOutTypes InitInstanceOfTestReferenceTypeWithOutTypes(TestReferenceTypeWithOutTypes? instance = null)
        {
            instance = instance ?? new TestReferenceTypeWithOutTypes();
            
            instance.String = "PublicString123#'!";
            instance.Int = 100;
            instance.TestEnum = TestEnum.Value;
            instance.ValueTypeArray = new[] { 1, 2, 3, 4, 5 };
            instance.ValueTypeCollection = new List<int> { 1, 2, 3, 4, 5 };
            instance.ReferenceTypeArray = new[] { new object(), new object(), new object() };
            instance.ReferenceTypeCollection = new[] { new object(), new object(), new object() };
            instance.ArrayOfNulls = new object?[] { null, null, null };
            instance.CollectionOfNulls = new List<object?> { null, null, null };

            instance.CyclicReference = instance;
            TestReferenceTypeWithOutTypes.StaticCyclicReference = instance;

            return instance;
        }
        
        [Serializable]
        private class TestReferenceTypeWithTypes : TestReferenceTypeWithOutTypes
        {
            #region System.Type
            
            public Type? Type { get; set; }
            
            public Array? TypeArray { get; set; }
            
            public ICollection<Type>? TypeCollection { get; set; }

            #endregion
        }

        [Serializable]
        private class TestReferenceTypeWithOutTypes
        {
            #region String
            
            public string? String { get; set; }
            
            #endregion
            
            #region ValueType
            
            public int Int { get; set; }

            public TestEnum TestEnum { get; set; }

            public Array? ValueTypeArray { get; set; }

            public ICollection<int>? ValueTypeCollection { get; set; }

            #endregion

            #region ReferenceType

            public Array? ReferenceTypeArray { get; set; }

            public ICollection<object>? ReferenceTypeCollection { get; set; }

            public TestReferenceTypeWithOutTypes? CyclicReference { get; set; }

            public static TestReferenceTypeWithOutTypes? StaticCyclicReference { get; set; }

            #endregion

            #region Nullable

            public int? NullableInt { get; } = null;
            
            public TestReferenceTypeWithOutTypes? NullableReference { get; } = null;
            
            public Array? ArrayOfNulls { get; set; }

            public ICollection<object?>? CollectionOfNulls { get; set; }
            
            #endregion
        }

        private enum TestEnum
        {
            Default = 0,
            Value = 1,
        }
    }
}