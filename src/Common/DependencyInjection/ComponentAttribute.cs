namespace SpaceEngineers.Core.DependencyInjection;

using System;
using System.Runtime.CompilerServices;

[CompilerGenerated]
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ComponentAttribute : Attribute
{
    public ComponentAttribute(EnLifestyle lifestyle)
    {
        Lifestyle = lifestyle;
    }

    public EnLifestyle Lifestyle { get; }
}