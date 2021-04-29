namespace SpaceEngineers.Core.AutoRegistration.Internals
{
    using System;
    using SimpleInjector;

    internal interface IRegistrationInfo
    {
        Type ServiceType { get; }

        Lifestyle Lifestyle { get; }
    }
}