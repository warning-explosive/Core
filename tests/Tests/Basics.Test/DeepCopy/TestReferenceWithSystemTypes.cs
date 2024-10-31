namespace SpaceEngineers.Core.Basics.Test.DeepCopy;

using System;
using System.Collections.Generic;

[Serializable]
public class TestReferenceWithSystemTypes : TestReferenceWithoutSystemTypes
{
    /*
     * System.Type
     */
    internal Type? Type { get; set; }

    internal Array? TypeArray { get; set; }

    internal ICollection<Type>? TypeCollection { get; set; }

    public static TestReferenceWithSystemTypes Create()
    {
        var instance = new TestReferenceWithSystemTypes
        {
            Type = typeof(TestReferenceWithoutSystemTypes),
            TypeArray = new[] { typeof(TestReferenceWithoutSystemTypes), typeof(string), typeof(int) },
            TypeCollection = new List<Type> { typeof(TestReferenceWithoutSystemTypes), typeof(string), typeof(int) }
        };

        CreateOrInit(instance);

        return instance;
    }
}