namespace SpaceEngineers.Core.Utilities.Test.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using Utilities.Extensions;
    using Xunit;
    using Xunit.Abstractions;

    public class ObjectExtensionsDeepCopyTest : TestBase
    {
        public ObjectExtensionsDeepCopyTest(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public void DeepCopyPerfomanceTest()
        {
            var original = new TestReferenceType1();
            InitInstanceOfTestReferenceType1(original);

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
            
                AssertTestReferenceType1(original, cloneBySerialization);
                AssertTestReferenceType1(original, cloneByReflection);
            
                Output.WriteLine($"Profit = {sw1.ElapsedTicks / sw2.ElapsedTicks}");
                Assert.True(sw1.ElapsedTicks > sw2.ElapsedTicks);
            }
            
            SingleBatch();
            SingleBatch();
            SingleBatch();
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
            var original = new TestReferenceType2();
            InitInstanceOfTestReferenceType1(original);
            InitInstanceOfTestReferenceType2(original);
            
            var clone = original.DeepCopy();
            AssertTestReferenceType1(original, clone);
            AssertTestReferenceType2(original, clone);
        }
        
        [Fact]
        public void DeepCopyBySerializationThrowsTest()
        {
            var original = new TestReferenceType2();
            InitInstanceOfTestReferenceType1(original);
            InitInstanceOfTestReferenceType2(original);
            
            Assert.Throws<SerializationException>(() => original.DeepCopyBySerialization());
        }
        
        [Fact]
        public void DeepCopyBySerializationTest()
        {
            var original = new TestReferenceType1();
            InitInstanceOfTestReferenceType1(original);
            
            var clone = original.DeepCopyBySerialization();
            AssertTestReferenceType1(original, clone);
        }

        private static void AssertTestReferenceType2(TestReferenceType2 original, TestReferenceType2 clone)
        {
            #region Type
            
            Assert.True(original.GetType() == clone.GetType());
            Assert.True(ReferenceEquals(original.GetType(),clone.GetType()));
            Assert.True(original.Type == clone.Type);
            Assert.True(ReferenceEquals(original.Type, clone.Type));

            Assert.False(original.TypeArray.Equals(clone.TypeArray));
            Assert.False(ReferenceEquals(original.TypeArray, clone.TypeArray));

            Assert.False(original.TypeCollection.Equals(clone.TypeCollection));
            Assert.False(ReferenceEquals(original.TypeCollection, clone.TypeCollection));
            Assert.True(original.TypeCollection.SequenceEqual(clone.TypeCollection));

            #endregion
        }

        private static void AssertTestReferenceType1(TestReferenceType1 original, TestReferenceType1 clone)
        {
            #region String
            
            Assert.Equal(original.String, clone.String);
            Assert.False(ReferenceEquals(original.String, clone.String));

            #endregion
            
            #region ValueType
            
            Assert.Equal(original.Int, clone.Int);
            
            Assert.Equal(original.TestEnum, clone.TestEnum);
            
            Assert.False(original.ValueTypeArray.Equals(clone.ValueTypeArray));
            Assert.False(ReferenceEquals(original.ValueTypeArray, clone.ValueTypeArray));

            Assert.False(original.ValueTypeCollection.Equals(clone.ValueTypeCollection));
            Assert.False(ReferenceEquals(original.ValueTypeCollection, clone.ValueTypeCollection));
            Assert.True(original.ValueTypeCollection.SequenceEqual(clone.ValueTypeCollection));

            #endregion

            #region ReferenceType
            
            Assert.False(original.ReferenceTypeArray.Equals(clone.ReferenceTypeArray));
            Assert.False(ReferenceEquals(original.ReferenceTypeArray, clone.ReferenceTypeArray));
            
            Assert.False(original.ReferenceTypeCollection.Equals(clone.ReferenceTypeCollection));
            Assert.False(ReferenceEquals(original.ReferenceTypeCollection, clone.ReferenceTypeCollection));
            Assert.False(original.ReferenceTypeCollection.SequenceEqual(clone.ReferenceTypeCollection));
         
            Assert.True(original.Equals(original.CyclicReference));
            Assert.True(ReferenceEquals(original, original.CyclicReference));
            Assert.True(clone.Equals(clone.CyclicReference));
            Assert.True(ReferenceEquals(clone, clone.CyclicReference));
            
            Assert.False(original.Equals(clone));
            Assert.False(ReferenceEquals(original, clone));
            
            Assert.False(original.Equals(clone.CyclicReference));
            Assert.False(ReferenceEquals(original, clone.CyclicReference));
            
            Assert.False(original.CyclicReference.Equals(clone.CyclicReference));
            Assert.False(ReferenceEquals(original.CyclicReference, clone.CyclicReference));
            
            Assert.True(original.Equals(TestReferenceType1.StaticCyclicReference));
            Assert.True(ReferenceEquals(original, TestReferenceType1.StaticCyclicReference));
            Assert.False(clone.Equals(TestReferenceType1.StaticCyclicReference));
            Assert.False(ReferenceEquals(clone, TestReferenceType1.StaticCyclicReference));
            
            #endregion
        }

        private void InitInstanceOfTestReferenceType2(TestReferenceType2 instance)
        {
            instance.Type = typeof(TestReferenceType1);
            instance.TypeArray = new[] { typeof(TestReferenceType1), typeof(string), typeof(int) };
            instance.TypeCollection = new List<Type> { typeof(TestReferenceType1), typeof(string), typeof(int) };
        }

        private void InitInstanceOfTestReferenceType1(TestReferenceType1 instance)
        {
            instance.String = "PublicString123#'!";
            
            instance.Int = 100;
            instance.TestEnum = TestEnum.Value;
            instance.ValueTypeArray = new[] { 1, 2, 3, 4, 5 };
            instance.ValueTypeCollection = new List<int> { 1, 2, 3, 4, 5 };
            
            instance.ReferenceTypeArray = new[] { new object(), new object(), new object() };
            instance.ReferenceTypeCollection = new List<object> { new object(), new object(), new object() };
            instance.CyclicReference = instance;
            TestReferenceType1.StaticCyclicReference = instance;
        }

        [Serializable]
        private class TestReferenceType2 : TestReferenceType1
        {
            #region System.Type
            
            public Type Type { get; set; }
            
            public Array TypeArray { get; set; }
            
            public ICollection<Type> TypeCollection { get; set; }

            #endregion
        }
        
        [Serializable]
        private class TestReferenceType1
        {
            #region String
            
            public string String { get; set; }
            
            #endregion
            
            #region ValueType
            
            public int Int { get; set; }

            public TestEnum TestEnum { get; set; }

            public Array ValueTypeArray { get; set; }

            public ICollection<int> ValueTypeCollection { get; set; }

            #endregion

            #region ReferenceType

            public Array ReferenceTypeArray { get; set; }
            
            public ICollection<object> ReferenceTypeCollection { get; set; }

            public TestReferenceType1 CyclicReference { get; set; }

            public static TestReferenceType1 StaticCyclicReference { get; set; }

            #endregion
        }

        private enum TestEnum
        {
            Default = 0,
            Value = 1,
        }
    }
}