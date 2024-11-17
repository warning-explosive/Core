namespace SpaceEngineers.Core.DependencyInjection.Registration;

using System;
using Basics;

public class DecoratorDependencyInjectionRegistrationInfo : IDependencyInjectionRegistrationInfo,
                                                            IEquatable<DecoratorDependencyInjectionRegistrationInfo>,
                                                            ISafelyEquatable<DecoratorDependencyInjectionRegistrationInfo>
{
    public DecoratorDependencyInjectionRegistrationInfo(Type service, Type implementation, EnLifestyle lifestyle)
    {
        Service = service.GenericTypeDefinitionOrSelf();
        Implementation = implementation;
        Lifestyle = lifestyle;
    }

    public Type Service { get; }

    public Type Implementation { get; }

    public EnLifestyle Lifestyle { get; }

    #region IEquatable

    public static bool operator ==(DecoratorDependencyInjectionRegistrationInfo? left, DecoratorDependencyInjectionRegistrationInfo? right)
    {
        return Equatable.Equals(left, right);
    }

    public static bool operator !=(DecoratorDependencyInjectionRegistrationInfo? left, DecoratorDependencyInjectionRegistrationInfo? right)
    {
        return !Equatable.Equals(left, right);
    }

    public bool SafeEquals(DecoratorDependencyInjectionRegistrationInfo other)
    {
        return Service == other.Service
               && Implementation == other.Implementation
               && Lifestyle == other.Lifestyle;
    }

    public bool Equals(DecoratorDependencyInjectionRegistrationInfo? other)
    {
        return Equatable.Equals(this, other);
    }

    public override bool Equals(object? obj)
    {
        return Equatable.Equals(this, obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Service, Implementation, Lifestyle);
    }

    #endregion

    public override string ToString()
    {
        return (Service, Implementation, Lifestyle).ToString(" | ");
    }
}