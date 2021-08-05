#pragma warning disable SA1200
#pragma warning disable CS1591

using System.ComponentModel;

namespace System.Runtime.CompilerServices
{
    /*
     * The IsExternalInit type is only included in the net5.0 (and future) target frameworks.
     * When compiling against older target frameworks you will need to manually define this type.
     */

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class IsExternalInit
    {
    }
}

#pragma warning restore CS1591
#pragma warning restore SA1200