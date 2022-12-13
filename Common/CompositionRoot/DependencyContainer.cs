namespace SpaceEngineers.Core.CompositionRoot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using Basics;
    using Exceptions;
    using Extensions;
    using Registration;
    using SimpleInjector;
    using SimpleInjector.Lifestyles;
    using Verifiers;

    /// <summary>
    /// Dependency container implementation based on SimpleInjector
    /// Resolve dependencies by 'Dependency Injection' patterns
    /// Don't use it like ServiceLocator!
    /// </summary>
    [ManuallyRegisteredComponent("Is created manually and implicitly during DependencyContainer initialization")]
    public class DependencyContainer : IDisposable,
                                       IDependencyContainer,
                                       IResolvable<IDependencyContainer>
    {
        private DependencyContainerOptions _options;
        private bool _wasVerified;
        private bool _suppressResolveWarnings;

        /// <summary> .cctor </summary>
        /// <param name="typeProvider">ContainerDependentTypeProvider</param>
        /// <param name="options">DependencyContainerOptions</param>
        [Obsolete("Use factory methods instead")]
        public DependencyContainer(
            DependencyContainerOptions options,
            ITypeProvider typeProvider)
        {
            Container = new Container
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

            _options = options;
            _wasVerified = false;
            _suppressResolveWarnings = false;

            Configure(typeProvider);

            if (!options.ManualVerification)
            {
                Verify();
            }
        }

        /// <summary>
        /// Container
        /// </summary>
        public Container Container { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            Container.Dispose();
        }

        #region Creation

        /// <summary>
        /// Creates IDependencyContainer without assembly limitations
        /// </summary>
        /// <param name="options">DependencyContainer creation options</param>
        /// <returns>DependencyContainer</returns>
        public static IDependencyContainer Create(DependencyContainerOptions options)
        {
            var typeProvider = new TypeProvider(
                AssembliesExtensions.AllAssembliesFromCurrentDomain(),
                options.ExcludedAssemblies,
                options.ExcludedNamespaces,
                options.AdditionalOurTypes);

            #pragma warning disable 618

            return new DependencyContainer(options, typeProvider);

            #pragma warning restore 618
        }

        /// <summary>
        /// Creates IDependencyContainer bounded above by specified assemblies
        /// </summary>
        /// <param name="options">DependencyContainer creation options</param>
        /// <param name="assemblies">Assembly for container configuration</param>
        /// <returns>DependencyContainer</returns>
        public static IDependencyContainer CreateBoundedAbove(
            DependencyContainerOptions options,
            params Assembly[] assemblies)
        {
            var belowAssemblies = assemblies
                .SelectMany(assembly => AssembliesExtensions.AllAssembliesFromCurrentDomain().Below(assembly))
                .Distinct()
                .ToArray();

            var typeProvider = new TypeProvider(
                belowAssemblies,
                options.ExcludedAssemblies,
                options.ExcludedNamespaces,
                options.AdditionalOurTypes);

            #pragma warning disable 618

            return new DependencyContainer(options, typeProvider);

            #pragma warning restore 618
        }

        /// <summary>
        /// Creates IDependencyContainer exactly bounded by specified assemblies
        /// </summary>
        /// <param name="options">DependencyContainer creation options</param>
        /// <param name="assemblies">Assemblies for container configuration</param>
        /// <returns>DependencyContainer</returns>
        public static IDependencyContainer CreateExactlyBounded(
            DependencyContainerOptions options,
            params Assembly[] assemblies)
        {
            var typeProvider = new TypeProvider(
                assemblies,
                options.ExcludedAssemblies,
                options.ExcludedNamespaces,
                options.AdditionalOurTypes);

            #pragma warning disable 618

            return new DependencyContainer(options, typeProvider);

            #pragma warning restore 618
        }

        #endregion

        #region IScopedContainer

        /// <inheritdoc />
        public IDisposable OpenScope()
        {
            return AsyncScopedLifestyle.BeginScope(Container);
        }

        #if NETSTANDARD2_1

        /// <inheritdoc />
        public IAsyncDisposable OpenScopeAsync()
        {
            return AsyncScopedLifestyle.BeginScope(Container);
        }

        #endif

        #endregion

        #region IDependencyContainer

        /// <inheritdoc />
        public TService Resolve<TService>()
            where TService : class
        {
            return Resolve(typeof(TService),
                () =>
                {
                    IsNotInitializable(typeof(TService));
                    return Container.GetInstance<TService>();
                });
        }

        /// <inheritdoc />
        public TService Resolve<TService, TRuntimeInfo>(TRuntimeInfo runtimeInfo)
            where TService : class, IInitializable<TRuntimeInfo>
        {
            return Resolve(typeof(TService),
                () =>
                {
                    var instance = Container.GetInstance<TService>();

                    instance.Initialize(runtimeInfo);

                    return instance;
                });
        }

        /// <inheritdoc />
        public object Resolve(Type service)
        {
            return Resolve(service,
                () =>
                {
                    IsNotInitializable(service);
                    return Container.GetInstance(service);
                });
        }

        /// <inheritdoc />
        public object ResolveGeneric(Type service, params Type[] genericTypeArguments)
        {
            return Resolve(service,
                () =>
                {
                    IsNotInitializable(service);
                    return Container.GetInstance(service.MakeGenericType(genericTypeArguments));
                });
        }

        /// <inheritdoc />
        public IEnumerable<TService> ResolveCollection<TService>()
            where TService : class
        {
            return Resolve(typeof(TService),
                () =>
                {
                    IsNotInitializable(typeof(TService));
                    return Container.GetAllInstances<TService>();
                });
        }

        /// <inheritdoc />
        public IEnumerable<object> ResolveCollection(Type service)
        {
            return Resolve(service, () =>
            {
                IsNotInitializable(service);
                return Container.GetAllInstances(service);
            });
        }

        #endregion

        #region Internals

        /// <summary>
        /// Suppresses warnings on service resolution
        /// </summary>
        public void SuppressResolveWarnings()
        {
            _suppressResolveWarnings = true;
        }

        /// <summary>
        /// Verifies container's integrity
        /// </summary>
        public void Verify()
        {
            ExecutionExtensions
                .Try(() =>
                {
                    Container.GetAllInstances<IConfigurationVerifier>().Each(v => v.Verify());
                    _wasVerified = true;
                    _suppressResolveWarnings = false;
                })
                .Catch<Exception>(ex => throw new ContainerConfigurationException(ex))
                .Invoke();
        }

        private void Configure(ITypeProvider typeProvider)
        {
            ExecutionExtensions
                .Try(typeProvider, tp => Register(CollectRegistrations(tp)))
                .Catch<Exception>(ex => throw new ContainerConfigurationException(ex))
                .Invoke();
        }

        private IRegistrationsContainer CollectRegistrations(ITypeProvider typeProvider)
        {
            var overrides = new ComponentsOverrideContainer(_options.ConstructorResolutionBehavior);
            var servicesProvider = new AutoRegistrationServicesProvider(typeProvider);
            var autoRegistrations = new AutoRegistrationsContainer(typeProvider, servicesProvider, _options.ConstructorResolutionBehavior);
            var manualRegistrations = new ManualRegistrationsContainer(this, typeProvider);

            var registrations = new CompositeRegistrationsContainer(
                overrides,
                autoRegistrations,
                manualRegistrations);

            _options = _options
                .WithManualRegistrations(new TypeProviderManualRegistration(typeProvider, servicesProvider))
                .WithManualRegistrations(new DependencyContainerManualRegistration(this, _options))
                .WithManualRegistrations(new RegistrationsContainerManualRegistration(registrations, overrides));

            _options.ManualRegistrations.Each(registration => registration.Register(manualRegistrations));
            _options.Overrides.Each(overrideInfo => overrideInfo.RegisterOverrides(overrides));

            return registrations;
        }

        private void Register(IRegistrationsContainer registrations)
        {
            registrations.Instances().RegisterInstances(Container);
            registrations.Resolvable().RegisterResolvable(Container);
            registrations.Delegates().RegisterDelegates(Container);
            registrations.Collections().RegisterCollections(Container);
            registrations.Decorators().RegisterDecorators(Container);
        }

        private T Resolve<T>(Type service, Func<T> producer)
        {
            if (!_wasVerified && !_suppressResolveWarnings)
            {
                Console.WriteLine($"WRN: Trying to resolve '{service.FullName}' but dependency container isn't verified. Make sure you have set up auto verification on container creation to true '.WithManualVerification(false)' or you verified it manually.");
            }

            return ExecutionExtensions
                .Try(producer)
                .Catch<Exception>()
                .Invoke(ex => throw new ComponentResolutionException(service, ex));
        }

        private static void IsNotInitializable(Type serviceType)
        {
            if (serviceType.IsSubclassOfOpenGeneric(typeof(IInitializable<>)))
            {
                throw new InvalidOperationException($"Use overload of {nameof(IDependencyContainer.Resolve)} method with additional parameter to resolve initializable service {serviceType.FullName}");
            }
        }

        #endregion
    }
}