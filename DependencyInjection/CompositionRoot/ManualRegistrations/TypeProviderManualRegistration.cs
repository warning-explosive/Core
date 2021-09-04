namespace SpaceEngineers.Core.CompositionRoot.ManualRegistrations
{
    using System;
    using System.Linq;
    using Api.Abstractions;
    using Api.Abstractions.Registration;
    using Api.Extensions;
    using AutoRegistration.Api.Abstractions;
    using Implementations;

    internal class TypeProviderManualRegistration : IManualRegistration
    {
        private readonly ITypeProvider _typeProvider;
        private readonly IAutoRegistrationServicesProvider _servicesProvider;

        internal TypeProviderManualRegistration(
            ITypeProvider typeProvider,
            IAutoRegistrationServicesProvider servicesProvider)
        {
            _typeProvider = typeProvider;
            _servicesProvider = servicesProvider;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            var typeProviderInstance = _typeProvider.FlattenDecoratedObject().OfType<TypeProvider>().FirstOrDefault()
                ?? throw new InvalidOperationException($"You can't replace {typeof(ITypeProvider)} with custom implementation");

            container
                .RegisterInstance<ITypeProvider>(_typeProvider)
                .RegisterInstance<TypeProvider>(typeProviderInstance)
                .RegisterInstance<IAutoRegistrationServicesProvider>(_servicesProvider)
                .RegisterInstance(_servicesProvider.GetType(), _servicesProvider);
        }
    }
}