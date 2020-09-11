namespace SpaceEngineers.Core.AutoRegistration.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using Basics;
    using SimpleInjector;

    /// <summary>
    /// Wrapper around service that supports versions
    /// </summary>
    /// <typeparam name="TService">TService type-argument</typeparam>
    [ManualRegistration]
    internal class Versioned<TService> : IVersioned<TService>
        where TService : class
    {
        private readonly Container _container;
        private readonly IVersionedContainer _versionedContainer;
        private readonly IDictionary<Guid, InstanceProducer<TService>> _versionProducers;

        /// <summary> .cctor </summary>
        /// <param name="container">Container</param>
        /// <param name="versionedContainer">IDependencyContainer</param>
        /// <param name="original">original TService</param>
        /// <param name="versions">Supplied versions</param>
        public Versioned(Container container,
                         IVersionedContainer versionedContainer,
                         TService original,
                         IEnumerable<IVersionFor<TService>> versions)
        {
            _container = container;
            _versionedContainer = versionedContainer;
            Original = original;
            Versions = versions.Where(v => !v.GetType().IsSubclassOfOpenGeneric(typeof(VersionForStub<>)))
                               .Select(v => v.Version)
                               .ToList();
            _versionProducers = new Dictionary<Guid, InstanceProducer<TService>>();
        }

        /// <inheritdoc />
        public TService Current => SelectCurrentVersion();

        /// <inheritdoc />
        public TService Original { get; }

        /// <inheritdoc />
        public ICollection<TService> Versions { get; }

        private TService SelectCurrentVersion()
        {
            var appliedVersion = _versionedContainer.AppliedVersion<TService>();

            if (appliedVersion == null)
            {
                return Original;
            }

            var versionId = appliedVersion.VersionId;

            if (_versionProducers.TryGetValue(versionId, out var producer))
            {
                return producer.GetInstance();
            }

            Func<TService> appliedVersionInstanceFactory;
            if (appliedVersion.VersionInstanceFactory != null)
            {
                var typedFactory = (Func<IVersionFor<TService>>)appliedVersion.VersionInstanceFactory;
                appliedVersionInstanceFactory = () => typedFactory().Version;
            }
            else
            {
                var appliedVersionInstance = Versions.SingleOrDefault(v => v.GetType() == appliedVersion.VersionType);

                if (appliedVersionInstance == null)
                {
                    return Original;
                }

                appliedVersionInstanceFactory = () => appliedVersionInstance;
            }

            producer = InstanceProducer(appliedVersionInstanceFactory);
            _versionProducers[versionId] = producer;

            return producer.GetInstance();
        }

        private InstanceProducer<TService> InstanceProducer(Func<TService> factory)
        {
            return _container.GetRegistration(typeof(TService))
                             .EnsureNotNull($"{typeof(TService)} must be registered in the container")
                             .Lifestyle
                             .CreateProducer(factory, _container);
        }
    }
}