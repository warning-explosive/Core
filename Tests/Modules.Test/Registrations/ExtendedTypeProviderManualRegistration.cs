namespace SpaceEngineers.Core.Modules.Test.Registrations
{
    using System;
    using System.Collections.Generic;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions;
    using Mocks;

    internal class ExtendedTypeProviderManualRegistration : IManualRegistration
    {
        private readonly IReadOnlyCollection<Type> _additionalTypes;

        public ExtendedTypeProviderManualRegistration(IReadOnlyCollection<Type> additionalTypes)
        {
            _additionalTypes = additionalTypes;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container
                .RegisterDecorator<ITypeProvider, ExtendedTypeProviderDecorator>(EnLifestyle.Singleton)
                .RegisterInstance(new ExtendedTypeProviderDecorator.TypeProviderExtension(_additionalTypes));
        }
    }
}