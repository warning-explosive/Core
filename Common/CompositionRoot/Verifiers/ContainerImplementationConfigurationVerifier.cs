namespace SpaceEngineers.Core.CompositionRoot.Verifiers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using SimpleInjector;

    [Component(EnLifestyle.Singleton)]
    internal class ContainerImplementationConfigurationVerifier : IConfigurationVerifier,
                                                                  ICollectionResolvable<IConfigurationVerifier>
    {
        private readonly Container _container;

        public ContainerImplementationConfigurationVerifier(Container container)
        {
            _container = container;
        }

        [SuppressMessage("Analysis", "CA1031", Justification = "desired behavior")]
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
}