namespace SpaceEngineers.Core.DependencyInjection.Registration;

using System;

public interface IDependencyInjectionRegistrationInfo
{
    Type Service { get; }

    EnLifestyle Lifestyle { get; }
}