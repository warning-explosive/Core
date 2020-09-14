﻿namespace SpaceEngineers.Core.GenericEndpoint.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoRegistration.Abstractions;
    using NServiceBus.ObjectBuilder;

    /// <summary>
    /// Resolution adapter for IDependencyContainer managed in external mode
    /// </summary>
    internal class NServiceBusDependencyContainerResolutionAdapter : IBuilder
    {
        private readonly IDependencyContainer _container;

        private readonly IDisposable _cleanup;

        public NServiceBusDependencyContainerResolutionAdapter(IDependencyContainer container)
        {
            _container = container;
            _cleanup = _container.OpenScope();
        }

        public void Dispose()
        {
            _cleanup.Dispose();
        }

        public T Build<T>()
        {
            return (T)_container.Resolve(typeof(T));
        }

        public object Build(Type typeToBuild)
        {
            return _container.Resolve(typeToBuild);
        }

        public void BuildAndDispatch(Type typeToBuild, Action<object> action)
        {
            action.Invoke(_container.Resolve(typeToBuild));
        }

        public IEnumerable<T> BuildAll<T>()
        {
            return _container.ResolveCollection(typeof(T))
                             .OfType<T>();
        }

        public IEnumerable<object> BuildAll(Type typeToBuild)
        {
            return _container.ResolveCollection(typeToBuild);
        }

        public void Release(object instance)
        {
        }

        public IBuilder CreateChildBuilder()
        {
            return new NServiceBusDependencyContainerResolutionAdapter(_container);
        }
    }
}