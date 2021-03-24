namespace SpaceEngineers.Core.AutoRegistration.Internals
{
    using System;
    using System.Collections.Generic;

    internal interface IRegistrationsContainer
    {
        IReadOnlyCollection<(Type, object)> Singletons();

        IReadOnlyCollection<ServiceRegistrationInfo> Resolvable();

        IReadOnlyCollection<ServiceRegistrationInfo> Collections();

        IReadOnlyCollection<DecoratorRegistrationInfo> Decorators();
    }
}