namespace SpaceEngineers.Core.CompositionRoot.SimpleInjector.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Extensions;
    using global::SimpleInjector;
    using global::SimpleInjector.Lifestyles;
    using Registration;

    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    [ManuallyRegisteredComponent]
    internal class SimpleInjectorDependencyContainerImplementation : IDependencyContainerImplementation
    {
        private readonly Container _container;

        public SimpleInjectorDependencyContainerImplementation()
        {
            _container = new Container
            {
                Options =
                {
                    DefaultLifestyle = Lifestyle.Transient,
                    DefaultScopedLifestyle = new AsyncScopedLifestyle(),
                    UseFullyQualifiedTypeNames = true,
                    ResolveUnregisteredConcreteTypes = false,
                    AllowOverridingRegistrations = false,
                    SuppressLifestyleMismatchVerification = false,
                    UseStrictLifestyleMismatchBehavior = true,
                    EnableAutoVerification = true,
                }
            };
        }

        public void Dispose()
        {
            _container.Dispose();
        }

        public void Verify()
        {
            _container.Verify(VerificationOption.VerifyAndDiagnose);
        }

        #region Registration

        public void Register(Type service, Type implementation, EnLifestyle lifestyle)
        {
            _container.Register(service, implementation, lifestyle.MapLifestyle());
        }

        public void Register(Type service, Func<object> instanceProducer, EnLifestyle lifestyle)
        {
            _container.Register(service, instanceProducer, lifestyle.MapLifestyle());
        }

        public void RegisterInstance(Type service, object instance)
        {
            _container.RegisterInstance(service, instance);
        }

        public void RegisterOpenGenericFallBack(Type service, Type implementation, EnLifestyle lifestyle)
        {
            _container.RegisterConditional(service, implementation, lifestyle.MapLifestyle(), ctx => !ctx.Handled);
        }

        public void RegisterCollection(Type service, IEnumerable<Type> implementations, EnLifestyle lifestyle)
        {
            /*
             * Register each element of collection as implementation to provide lifestyle for container
             */

            var materialized = implementations.ToList();

            materialized
                .Select(implementation => new ServiceRegistrationInfo(service, implementation, lifestyle))
                .RegisterImplementationsWithOpenGenericFallBack(this);

            _container.Collection.Register(service, materialized.OrderByDependencyAttribute());
        }

        public void RegisterDecorator(Type service, Type implementation, EnLifestyle lifestyle)
        {
            _container.RegisterDecorator(service, implementation, lifestyle.MapLifestyle());
        }

        #endregion

        #region IScopedContainer

        public IDisposable OpenScope()
        {
            return AsyncScopedLifestyle.BeginScope(_container);
        }

        #if NETSTANDARD2_1

        public IAsyncDisposable OpenScopeAsync()
        {
            return AsyncScopedLifestyle.BeginScope(_container);
        }

        #endif

        #endregion

        #region IDependencyContainer

        public TService Resolve<TService>()
            where TService : class
        {
            return _container.GetInstance<TService>();
        }

        public TService Resolve<TService, TRuntimeInfo>(TRuntimeInfo runtimeInfo)
            where TService : class, IInitializable<TRuntimeInfo>
        {
            throw new InvalidOperationException($"Use {nameof(Resolve)} method override without specified {nameof(runtimeInfo)} argument");
        }

        public object Resolve(Type service)
        {
            return _container.GetInstance(service);
        }

        public IEnumerable<TService> ResolveCollection<TService>()
            where TService : class
        {
            return _container.GetAllInstances<TService>();
        }

        public IEnumerable<object> ResolveCollection(Type service)
        {
            return _container.GetAllInstances(service);
        }

        #endregion
    }
}