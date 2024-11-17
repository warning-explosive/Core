namespace SpaceEngineers.Core.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using Basics;
using Exceptions;
using Extensions;
using Registration;
using Verifiers;

public class DependencyContainer : IDependencyContainer, IDisposable
{
    public DependencyContainer(DependencyContainerOptions options)
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

        Options = options;

        Configure();

        Verify();
    }

    public DependencyContainerOptions Options { get; }

    public Container Container { get; }

    public void Dispose()
    {
        Container.Dispose();
    }

    #region IScopedContainer

    public IDisposable OpenScope()
    {
        return AsyncScopedLifestyle.BeginScope(Container);
    }

    public IAsyncDisposable OpenScopeAsync()
    {
        return AsyncScopedLifestyle.BeginScope(Container);
    }

    #endregion

    #region IDependencyContainer

    public TService Resolve<TService>()
        where TService : class
    {
        return Resolve(typeof(TService), () => Container.GetInstance<TService>());
    }

    public object Resolve(Type service)
    {
        return Resolve(service, () => Container.GetInstance(service));
    }

    public object ResolveGeneric(Type service, params Type[] genericTypeArguments)
    {
        return Resolve(service, () => Container.GetInstance(service.MakeGenericType(genericTypeArguments)));
    }

    public IEnumerable<TService> ResolveCollection<TService>()
        where TService : class
    {
        return Resolve(typeof(TService), () => Container.GetAllInstances<TService>());
    }

    public IEnumerable<object> ResolveCollection(Type service)
    {
        return Resolve(service, () => Container.GetAllInstances(service));
    }

    #endregion

    #region Internals

    private void Configure()
    {
        ExecutionExtensions
            .Try(de => Register(CollectRegistrations(de)), this)
            .Catch<Exception>(ex => throw new ContainerConfigurationException(ex))
            .Invoke();
    }

    private static IDependencyInjectionRegistrationContainer CollectRegistrations(DependencyContainer dependencyContainer)
    {
        var typesToScan = dependencyContainer
            .Options
            .Assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Concat(dependencyContainer.Options.Types)
            .Distinct();

        var generatedDependencyInjectionRegistrationTypes = typesToScan
            .Where(it => typeof(IDependencyInjectionRegistration).IsAssignableFrom(it) && it.IsCompilerGenerated());

        var generatedDependencyInjectionRegistrations = generatedDependencyInjectionRegistrationTypes
            .Select(Activator.CreateInstance)
            .OfType<IDependencyInjectionRegistration>()
            .ToArray();

        dependencyContainer.Options
            .WithManualRegistrations(generatedDependencyInjectionRegistrations)
            .WithManualRegistrations(new DependencyContainerRegistration(dependencyContainer, dependencyContainer.Options));

        var dependencyInjectionRegistrationContainer = new DependencyInjectionRegistrationContainer(dependencyContainer);

        dependencyContainer.Options.ManualRegistrations.Each(registration => registration.Register(dependencyInjectionRegistrationContainer));

        return dependencyInjectionRegistrationContainer;
    }

    private void Register(IDependencyInjectionRegistrationContainer dependencyInjectionRegistrationContainer)
    {
        Container.Register(dependencyInjectionRegistrationContainer.Dependencies());

        Container.RegisterCollections(dependencyInjectionRegistrationContainer.Dependencies());

        Container.RegisterDecorators(dependencyInjectionRegistrationContainer.Decorators());
    }

    private void Verify()
    {
        ExecutionExtensions
            .Try(VerifyUnsafe, Container)
            .Catch<Exception>(ex => throw new ContainerConfigurationException(ex))
            .Invoke();

        static void VerifyUnsafe(Container container)
        {
            container
                .GetAllInstances<IDependencyInjectionConfigurationVerifier>()
                .Each(v => v.Verify());
        }
    }

    private static T Resolve<T>(Type service, Func<T> producer)
    {
        return producer
            .Try()
            .Catch<Exception>()
            .Invoke(ex => throw new ComponentResolutionException(service, ex));
    }

    #endregion
}