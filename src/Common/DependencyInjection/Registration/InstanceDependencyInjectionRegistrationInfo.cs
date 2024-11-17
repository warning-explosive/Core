namespace SpaceEngineers.Core.DependencyInjection.Registration;

using System;
using Basics;

public class InstanceDependencyInjectionRegistrationInfo : IDependencyInjectionDependencyRegistrationInfo,
                                                           IEquatable<InstanceDependencyInjectionRegistrationInfo>,
                                                           ISafelyEquatable<InstanceDependencyInjectionRegistrationInfo>
{
    public InstanceDependencyInjectionRegistrationInfo(Type service, object instance)
    {
        Service = service;
        Instance = instance;
    }

    public Type Service { get; }

    public object Instance { get; }

    public EnLifestyle Lifestyle => EnLifestyle.Singleton;

    #region IEquatable

    public static bool operator ==(InstanceDependencyInjectionRegistrationInfo? left, InstanceDependencyInjectionRegistrationInfo? right)
    {
        return Equatable.Equals(left, right);
    }

    public static bool operator !=(InstanceDependencyInjectionRegistrationInfo? left, InstanceDependencyInjectionRegistrationInfo? right)
    {
        return !Equatable.Equals(left, right);
    }

    public bool SafeEquals(InstanceDependencyInjectionRegistrationInfo other)
    {
        return Service == other.Service
               && Instance == other.Instance;
    }

    public bool Equals(InstanceDependencyInjectionRegistrationInfo? other)
    {
        return Equatable.Equals(this, other);
    }

    public override bool Equals(object? obj)
    {
        return Equatable.Equals(this, obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Service, Instance);
    }

    #endregion

    public override string ToString()
    {
        return (Service, Instance.GetType(), Lifestyle).ToString(" | ");
    }
}