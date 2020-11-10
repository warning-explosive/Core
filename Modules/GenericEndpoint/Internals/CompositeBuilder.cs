﻿namespace SpaceEngineers.Core.GenericEndpoint.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reactive.Disposables;
    using AutoRegistration.Abstractions;
    using Basics;
    using NServiceBus;
    using NServiceBus.ObjectBuilder;
    using NServiceBus.ObjectBuilder.Common;
    using SimpleInjector;

    /// <summary>
    /// Resolution adapter for IDependencyContainer managed in external mode
    /// </summary>
    [SuppressMessage("Regions", "SA1124", Justification = "Integration composition")]
    internal class CompositeBuilder : IBuilder, IConfigureComponents
    {
        private const string LightInjectObjectBuilder = "NServiceBus.LightInjectObjectBuilder";
        private const string CommonObjectBuilder = "NServiceBus.CommonObjectBuilder";

        private readonly IBuilder _builder;

        private readonly IConfigureComponents _configure;

        private readonly IDependencyContainer _container;

        private readonly IDisposable _cleanup;

        /// <summary> .cctor </summary>
        /// <param name="dependencyContainer">IDependencyContainer</param>
        public CompositeBuilder(IDependencyContainer dependencyContainer)
            : this(Builder(), dependencyContainer)
        {
        }

        private CompositeBuilder(IBuilder builder, IDependencyContainer dependencyContainer)
        {
            _builder = builder;
            _configure = (IConfigureComponents)_builder;

            _container = dependencyContainer;
            var scope = _container.OpenScope();

            _cleanup = Disposable.Create(scope, s => s.Dispose());
        }

        #region IBuilder

        public void Dispose()
        {
            _cleanup.Dispose();
        }

        public T Build<T>()
        {
            return new Func<T>(() => (T)_container.Resolve(typeof(T)))
                  .Try()
                  .Catch<ActivationException>()
                  .Invoke()
                ?? _builder.Build<T>();
        }

        public object Build(Type typeToBuild)
        {
            return new Func<object>(() => _container.Resolve(typeToBuild))
                  .Try()
                  .Catch<ActivationException>()
                  .Invoke()
                ?? _builder.Build(typeToBuild);
        }

        public void BuildAndDispatch(Type typeToBuild, Action<object> action)
        {
            action.Invoke(Build(typeToBuild));
        }

        public IEnumerable<T> BuildAll<T>()
        {
            return new Func<IEnumerable<T>>(() => _container.ResolveCollection(typeof(T)).OfType<T>())
                  .Try()
                  .Catch<ActivationException>()
                  .Invoke()
                ?? _builder.BuildAll<T>();
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
            return new CompositeBuilder(_builder.CreateChildBuilder(), _container);
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

        private static IBuilder Builder()
        {
            var assembly = typeof(Endpoint).Assembly;
            var container = (IContainer)Activator.CreateInstance(assembly.GetType(LightInjectObjectBuilder));
            return (IBuilder)Activator.CreateInstance(assembly.GetType(CommonObjectBuilder), container);
        }
    }
}