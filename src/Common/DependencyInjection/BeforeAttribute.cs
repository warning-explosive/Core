namespace SpaceEngineers.Core.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using SpaceEngineers.Core.Basics;

[CompilerGenerated]
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class BeforeAttribute : Attribute
{
    public BeforeAttribute(Type type, params Type[] types)
    {
        Types = new List<Type>(types) { type };
    }

    public BeforeAttribute(string type, params string[] types)
    {
        Types = new List<string>(types) { type }
            .Select(static type => TypeExtensions.FindType(type))
            .ToArray();
    }

    public IReadOnlyCollection<Type> Types { get; }
}