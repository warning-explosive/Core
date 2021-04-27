namespace SpaceEngineers.Core.AutoRegistration.Internals
{
    using System;
    using System.Collections.Generic;

    internal interface IRegistrationsContainer
    {
        IEnumerable<(Type, object)> Singletons();

        IEnumerable<ServiceRegistrationInfo> Resolvable();

        IEnumerable<ServiceRegistrationInfo> Collections();

        IEnumerable<DecoratorRegistrationInfo> Decorators();
    }
}