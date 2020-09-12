namespace SpaceEngineers.Core.AutoWiringApi.Attributes
{
    using System;

    /// <summary>
    /// Open generic fallback attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class OpenGenericFallBackAttribute : Attribute
    {
        /// <summary> .cctor </summary>
        public OpenGenericFallBackAttribute()
        {
        }
    }
}