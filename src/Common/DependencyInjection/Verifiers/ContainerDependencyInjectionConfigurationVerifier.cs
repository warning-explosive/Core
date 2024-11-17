namespace SpaceEngineers.Core.DependencyInjection.Verifiers;

using System;
using System.Collections.Generic;
using System.Linq;
using SimpleInjector;

[Component(EnLifestyle.Singleton)]
public class ContainerDependencyInjectionConfigurationVerifier : IDependencyInjectionConfigurationVerifier
{
    private readonly Container _container;

    public ContainerDependencyInjectionConfigurationVerifier(Container container)
    {
        _container = container;
    }

    public void Verify()
    {
        var exceptions = new List<Exception>();

        try
        {
            _container.Verify(VerificationOption.VerifyAndDiagnose);
        }
        catch (Exception exception)
        {
            exceptions.Add(exception);
        }

        if (exceptions.Any())
        {
            throw new AggregateException(exceptions);
        }
    }
}