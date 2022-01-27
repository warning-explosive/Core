namespace SpaceEngineers.Core.CompositionRoot.SimpleInjector.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Api.Abstractions;
    using Api.Abstractions.Container;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using global::SimpleInjector;
    using global::SimpleInjector.Lifestyles;

    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    [ManuallyRegisteredComponent("Is created manually during DependencyContainer initialization")]
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
                    EnableAutoVerification = false
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

        public void RegisterInstance(Type service, object instance)
        {
            _container.RegisterInstance(service, instance);
        }

        public void RegisterDelegate(Type service, Func<object> instanceProducer, EnLifestyle lifestyle)
        {
            _container.Register(service, instanceProducer, lifestyle.MapLifestyle());
        }

        public void RegisterOpenGenericFallBack(Type service, Type implementation, EnLifestyle lifestyle)
        {
            _container.RegisterConditional(service, implementation, lifestyle.MapLifestyle(), ctx => !ctx.Handled);
        }

        public void RegisterCollectionEntry(Type service, Type implementation, EnLifestyle lifestyle)
        {
            Register(implementation, implementation, lifestyle);
            _container.Collection.Append(service, implementation, lifestyle.MapLifestyle());
        }

        public void RegisterCollectionEntryInstance(Type service, object collectionEntryInstance)
        {
            RegisterInstance(collectionEntryInstance.GetType(), collectionEntryInstance);
            _container.Collection.AppendInstance(service, collectionEntryInstance);
        }

        public void RegisterCollectionEntryDelegate(Type service, Func<object> instanceProducer, EnLifestyle lifestyle)
        {
            AppendCollectionInstanceProducer(service, instanceProducer, lifestyle);
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

        public object ResolveGeneric(Type service, params Type[] genericTypeArguments)
        {
            return _container.GetInstance(service.MakeGenericType(genericTypeArguments));
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

        #region internals

        private void AppendCollectionInstanceProducer(Type service, Func<object> instanceProducer, EnLifestyle lifestyle)
        {
            this
                .CallMethod(nameof(AppendCollectionInstanceProducer))
                .WithTypeArgument(service)
                .WithArgument(instanceProducer)
                .WithArgument(lifestyle)
                .Invoke();
        }

        private void AppendCollectionInstanceProducer<TService>(Func<TService> instanceProducer, EnLifestyle lifestyle)
            where TService : class
        {
            _container.Collection.Append(instanceProducer, lifestyle.MapLifestyle());
        }

        #endregion
    }
}