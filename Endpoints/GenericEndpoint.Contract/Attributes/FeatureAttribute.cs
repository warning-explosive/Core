namespace SpaceEngineers.Core.GenericEndpoint.Contract.Attributes
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Specifies code feature
    /// Applicable to integration messages and web-api methods
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class FeatureAttribute : Attribute
    {
        /// <summary> .cctor </summary>
        /// <param name="feature">Required feature</param>
        /// <param name="features">Optional features</param>
        public FeatureAttribute(string feature, params string[] features)
        {
            Features = new HashSet<string>(features, StringComparer.OrdinalIgnoreCase) { feature };
        }

        /// <summary>
        /// Features
        /// </summary>
        public IReadOnlyCollection<string> Features { get; }
    }
}