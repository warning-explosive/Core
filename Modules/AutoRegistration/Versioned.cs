namespace SpaceEngineers.Core.AutoRegistration
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
    public class Versioned<TService> : IVersioned<TService>
        where TService : class
    {
        private readonly Container _container;
        private readonly IVersionedContainer _versionedContainer;
        private readonly IDictionary<Type, InstanceProducer<TService>> _versionProducers;

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
            Versions = versions.Select(v => v.Version).ToList();
            _versionProducers = new Dictionary<Type, InstanceProducer<TService>>();
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

            if (!_versionProducers.TryGetValue(appliedVersion, out var producer))
            {
                var appliedVersionInstance = Versions.Single(v => v.GetType() == appliedVersion);

                producer = _container.GetRegistration(typeof(TService))
                                     .EnsureNotNull($"{typeof(TService)} must be registered in the container")
                                     .Lifestyle
                                     .CreateProducer(() => appliedVersionInstance, _container);

                _versionProducers[appliedVersion] = producer;
            }

            return producer.GetInstance();
        }
    }
}