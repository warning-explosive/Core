namespace SpaceEngineers.Core.AutoRegistration
{
    using System;

    /// <summary>
    /// Information about service version
    /// </summary>
    public class VersionInfo
    {
        /// <summary> .cctor </summary>
        /// <param name="serviceType">Service type</param>
        /// <param name="versionType">Version type</param>
        /// <param name="versionInstanceFactory">Version instance factory</param>
        public VersionInfo(Type serviceType, Type? versionType, Func<object>? versionInstanceFactory)
        {
            VersionId = Guid.NewGuid();
            ServiceType = serviceType;
            VersionType = versionType;
            VersionInstanceFactory = versionInstanceFactory;
        }

        /// <summary>
        /// Version Id
        /// </summary>
        public Guid VersionId { get; }

        /// <summary>
        /// Service type
        /// </summary>
        public Type ServiceType { get; }

        /// <summary>
        /// Version type
        /// </summary>
        public Type? VersionType { get; }

        /// <summary>
        /// Version instance factory
        /// </summary>
        public Func<object>? VersionInstanceFactory { get; }
    }
}