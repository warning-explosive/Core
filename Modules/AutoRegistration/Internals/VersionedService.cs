namespace SpaceEngineers.Core.AutoRegistration.Internals
{
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;

    /// <summary>
    /// Wrapper around service that supports versions
    /// </summary>
    /// <typeparam name="TService">TService type-argument</typeparam>
    [ManualRegistration]
    public class VersionedService<TService> : IVersionedService<TService>
        where TService : class
    {
        private readonly IVersionedContainer _container;

        /// <summary> .cctor </summary>
        /// <param name="container">IDependencyContainer</param>
        /// <param name="original">original TService</param>
        /// <param name="versions">Supplied versions</param>
        public VersionedService(IVersionedContainer container, TService original, IEnumerable<IVersionFor<TService>> versions)
        {
            _container = container;
            Original = original;
            Versions = versions.Select(v => v.Version).ToList();
        }

        /// <inheritdoc />
        public TService Current => SelectCurrentVersion();

        /// <inheritdoc />
        public TService Original { get; }

        /// <inheritdoc />
        public ICollection<TService> Versions { get; }

        private TService SelectCurrentVersion()
        {
            var appliedVersion = _container.AppliedVersion<TService>();

            if (appliedVersion == null)
            {
                return Original;
            }

            return Versions.FirstOrDefault(v => v.GetType() == appliedVersion) ?? Original;
        }
    }
}