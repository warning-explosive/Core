namespace SpaceEngineers.Core.Basics.EqualityComparers
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <inheritdoc />
    public class AssemblyByNameEqualityComparer : EqualityComparer<Assembly>
    {
        /// <inheritdoc />
        public override bool Equals(Assembly x, Assembly y)
        {
            return string.Intern(x.GetName().FullName).Equals(string.Intern(y.GetName().FullName), StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public override int GetHashCode(Assembly obj)
        {
            return obj.GetName().FullName.GetHashCode(StringComparison.Ordinal);
        }
    }
}