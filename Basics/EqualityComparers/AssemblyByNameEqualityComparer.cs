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
            return x.GetName().FullName.Equals(y.GetName().FullName, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public override int GetHashCode(Assembly obj)
        {
            return obj.GetName().FullName.GetHashCode(StringComparison.InvariantCulture);
        }
    }
}