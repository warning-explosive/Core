namespace SpaceEngineers.Core.Basics.EqualityComparers;

using System;
using System.Collections.Generic;
using System.Reflection;

public class AssemblyByNameEqualityComparer : EqualityComparer<Assembly>
{
    public override bool Equals(Assembly x, Assembly y)
    {
        return string.Intern(x.GetName().FullName).Equals(string.Intern(y.GetName().FullName), StringComparison.Ordinal);
    }

    public override int GetHashCode(Assembly obj)
    {
        return obj.GetName().FullName.GetHashCode(StringComparison.Ordinal);
    }
}