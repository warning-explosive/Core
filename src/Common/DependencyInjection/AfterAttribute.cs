namespace SpaceEngineers.Core.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Basics;

[CompilerGenerated]
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class AfterAttribute : Attribute
{
    public AfterAttribute(Type type, params Type[] types)
    {
        Types = new List<Type>(types) { type };
    }

    public AfterAttribute(string type, params string[] types)
    {
        Types = new List<string>(types) { type }
            .Select(static type => TypeExtensions.FindType(type))
            .ToArray();
    }

    public IReadOnlyCollection<Type> Types { get; }
}