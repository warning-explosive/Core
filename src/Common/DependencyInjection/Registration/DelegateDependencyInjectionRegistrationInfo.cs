namespace SpaceEngineers.Core.DependencyInjection.Registration;

using System;
using Basics;

public class DelegateDependencyInjectionRegistrationInfo : IDependencyInjectionDependencyRegistrationInfo,
                                                           IEquatable<DelegateDependencyInjectionRegistrationInfo>,
                                                           ISafelyEquatable<DelegateDependencyInjectionRegistrationInfo>
{
    private readonly Func<object> _instanceProducer;

    private object? _instance;

    public DelegateDependencyInjectionRegistrationInfo(Type service, Func<object> instanceProducer, EnLifestyle lifestyle)
    {
        if (lifestyle != EnLifestyle.Singleton)
        {
            throw new NotSupportedException($"Delegates support only {EnLifestyle.Singleton} lifestyle due to the fact that they are always capture static state at application's startup");
        }

        Service = service.GenericTypeDefinitionOrSelf();
        Lifestyle = lifestyle;

        _instanceProducer = instanceProducer;
    }

    public Type Service { get; }

    public EnLifestyle Lifestyle { get; }

    #region IEquatable

    public static bool operator ==(DelegateDependencyInjectionRegistrationInfo? left, DelegateDependencyInjectionRegistrationInfo? right)
    {
        return Equatable.Equals(left, right);
    }

    public static bool operator !=(DelegateDependencyInjectionRegistrationInfo? left, DelegateDependencyInjectionRegistrationInfo? right)
    {
        return !Equatable.Equals(left, right);
    }

    public bool SafeEquals(DelegateDependencyInjectionRegistrationInfo other)
    {
        return Service == other.Service
               && _instanceProducer == other._instanceProducer
               && Lifestyle == other.Lifestyle;
    }

    public bool Equals(DelegateDependencyInjectionRegistrationInfo? other)
    {
        return Equatable.Equals(this, other);
    }

    public override bool Equals(object? obj)
    {
        return Equatable.Equals(this, obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Service, _instanceProducer, Lifestyle);
    }

    #endregion

    public override string ToString()
    {
        return (Service, Lifestyle).ToString(" | ");
    }

    public object InstanceProducer()
    {
        _instance ??= _instanceProducer();
        return _instance;
    }
}