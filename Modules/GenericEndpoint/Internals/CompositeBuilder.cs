﻿namespace SpaceEngineers.Core.GenericEndpoint.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using AutoRegistration.Abstractions;
    using NServiceBus;
    using NServiceBus.ObjectBuilder;

    /// <summary>
    /// Resolution adapter for IDependencyContainer managed in external mode
    /// </summary>
    [SuppressMessage("Regions", "SA1124", Justification = "Integration composition")]
    internal class CompositeBuilder : IBuilder, IConfigureComponents
    {
        private readonly IBuilder _builder;

        private readonly IConfigureComponents _configure;

        private readonly IDependencyContainer _container;

        private readonly IDisposable _cleanup;

        public CompositeBuilder(IBuilder builder, IConfigureComponents configure, IDependencyContainer container)
        {
            _builder = builder;
            _configure = configure;
            _container = container;
            _cleanup = _container.OpenScope();
        }

        #region IBuilder

        public void Dispose()
        {
            _cleanup.Dispose();
            _builder.Dispose();
        }

        public T Build<T>()
        {
            // TODO
            return _builder.Build<T>()
                 ?? (T)_container.Resolve(typeof(T));
        }

        public object Build(Type typeToBuild)
        {
            // TODO
            return _builder.Build(typeToBuild)
                ?? _container.Resolve(typeToBuild);
        }

        public void BuildAndDispatch(Type typeToBuild, Action<object> action)
        {
            action.Invoke(Build(typeToBuild));
        }

        public IEnumerable<T> BuildAll<T>()
        {
            // TODO
            return _builder.BuildAll<T>()
                ?? _container.ResolveCollection(typeof(T))
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
            return new CompositeBuilder(_builder, _configure, _container);
        }

        #endregion

        #region IConfigureComponents

        public void ConfigureComponent(Type concreteComponent, DependencyLifecycle dependencyLifecycle)
        {
            _configure.ConfigureComponent(concreteComponent, dependencyLifecycle);
        }

        public void ConfigureComponent<T>(DependencyLifecycle dependencyLifecycle)
        {
            _configure.ConfigureComponent<T>(dependencyLifecycle);
        }

        public void ConfigureComponent<T>(Func<T> componentFactory, DependencyLifecycle dependencyLifecycle)
        {
            _configure.ConfigureComponent(componentFactory, dependencyLifecycle);
        }

        public void ConfigureComponent<T>(Func<IBuilder, T> componentFactory, DependencyLifecycle dependencyLifecycle)
        {
            _configure.ConfigureComponent(componentFactory, dependencyLifecycle);
        }

        public void RegisterSingleton(Type lookupType, object instance)
        {
            _configure.RegisterSingleton(lookupType, instance);
        }

        public void RegisterSingleton<T>(T instance)
        {
            _configure.RegisterSingleton(instance);
        }

        public bool HasComponent<T>()
        {
            return _configure.HasComponent<T>();
        }

        public bool HasComponent(Type componentType)
        {
            return _configure.HasComponent(componentType);
        }

        #endregion
    }
}