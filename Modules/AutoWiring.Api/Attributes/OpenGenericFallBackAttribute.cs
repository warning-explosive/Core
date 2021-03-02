namespace SpaceEngineers.Core.AutoWiring.Api.Attributes
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