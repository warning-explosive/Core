namespace SpaceEngineers.Core.AutoRegistration.Internals
{
    using System;
    using System.Collections.Generic;
    using Abstractions;

    internal interface IExtendedManualRegistrationsContainer : IManualRegistrationsContainer
    {
        IReadOnlyCollection<(Type, object)> Singletons();

        IReadOnlyCollection<ServiceRegistrationInfo> Resolvable();

        IReadOnlyCollection<ServiceRegistrationInfo> Collections();

        IReadOnlyCollection<DecoratorRegistrationInfo> Decorators();
    }
}