namespace SpaceEngineers.Core.Basics.Test.DeepCopy
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// TestReferenceWithoutSystemTypes
    /// </summary>
    [SuppressMessage("Microsoft.NetCore.Analyzers", "CA5362", Justification = "For test reasons")]
    [Serializable]
    public class TestReferenceWithoutSystemTypes
    {
        /*
         * String
         */
        internal string? String { get; set; }

        /*
         * ValueType
         */
        internal int Int { get; set; }

        internal TestEnum TestEnum { get; set; }

        internal Array? ValueTypeArray { get; set; }

        internal ICollection<int>? ValueTypeCollection { get; set; }

        /*
         * ReferenceType
         */
        internal Array? ReferenceTypeArray { get; set; }

        internal ICollection<object>? ReferenceTypeCollection { get; set; }

        internal TestReferenceWithoutSystemTypes? CyclicReference { get; set; }

        internal static TestReferenceWithoutSystemTypes? StaticCyclicReference { get; set; }

        /*
         * Nullable
         */
        internal int? NullableInt { get; }

        internal TestReferenceWithoutSystemTypes? NullableReference { get; }

        internal Array? ArrayOfNulls { get; set; }

        internal ICollection<object?>? CollectionOfNulls { get; set; }

        /// <summary>
        /// CreateOrInit TestReferenceWithoutSystemTypes instance
        /// </summary>
        /// <param name="instance">TestReferenceWithoutSystemTypes (optional)</param>
        /// <returns>Initialized TestReferenceWithoutSystemTypes instance</returns>
        public static TestReferenceWithoutSystemTypes CreateOrInit(TestReferenceWithoutSystemTypes? instance = null)
        {
            instance ??= new TestReferenceWithoutSystemTypes();

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
            StaticCyclicReference = instance;

            return instance;
        }
    }
}