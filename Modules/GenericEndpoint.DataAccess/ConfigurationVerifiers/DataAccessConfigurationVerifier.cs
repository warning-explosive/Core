namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.ConfigurationVerifiers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions;
    using Core.DataAccess.Api.Model;

    [Component(EnLifestyle.Singleton)]
    internal class DataAccessConfigurationVerifier : IConfigurationVerifier,
                                                     ICollectionResolvable<IConfigurationVerifier>
    {
        private readonly ITypeProvider _typeProvider;
        private readonly IConstructorResolutionBehavior _constructorResolutionBehavior;

        public DataAccessConfigurationVerifier(
            ITypeProvider typeProvider,
            IConstructorResolutionBehavior constructorResolutionBehavior)
        {
            _typeProvider = typeProvider;
            _constructorResolutionBehavior = constructorResolutionBehavior;
        }

        public void Verify()
        {
            var isExternalInitTypes = new[]
            {
                AssembliesExtensions.FindRequiredType("System.Private.CoreLib", AssembliesExtensions.BuildName(nameof(System), nameof(System.Runtime), nameof(System.Runtime.CompilerServices), nameof(IsExternalInit))),
                typeof(IsExternalInit)
            };

            _typeProvider
                .OurTypes
                .Where(type => (typeof(IInlinedObject).IsAssignableFrom(type)
                                || type.IsSubclassOfOpenGeneric(typeof(IDatabaseEntity<>)))
                               && type.IsConcreteType())
                .SelectMany(type => Verify(type, isExternalInitTypes))
                .Each(exception => throw exception.Rethrow());
        }

        private IEnumerable<Exception> Verify(Type type, Type[] isExternalInitTypes)
        {
            if (TypeHasWrongConstructor(type, out var constructorException))
            {
                yield return constructorException;
            }

            if (TypeHasWrongModifier(type, out var modifierException))
            {
                yield return modifierException;
            }

            foreach (var initPropertyException in InitPropertiesExceptions(type, isExternalInitTypes))
            {
                yield return initPropertyException;
            }
        }

        private bool TypeHasWrongConstructor(Type type, [NotNullWhen(true)] out Exception? exception)
        {
            if (!_constructorResolutionBehavior.TryGetConstructor(type, out _))
            {
                exception = new InvalidOperationException($"Type {type} should have one public constructor");
                return true;
            }

            exception = default;
            return false;
        }

        private static bool TypeHasWrongModifier(Type type, [NotNullWhen(true)] out Exception? exception)
        {
            var hasGeneratedEqualityContract = ((TypeInfo)type)
                .DeclaredProperties
               ?.Where(property => property.Name.Equals("EqualityContract", StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault()
               ?.GetMethod
               ?.GetCustomAttribute(typeof(CompilerGeneratedAttribute)) != null;

            var hasCopyMethod = type.GetMethod("<Clone>$") != null;

            var isRecord = hasGeneratedEqualityContract && hasCopyMethod;

            if (!isRecord)
            {
                exception = new InvalidOperationException($"Type {type} should be defined as record");
                return true;
            }

            exception = default;
            return false;
        }

        private static IEnumerable<Exception> InitPropertiesExceptions(Type type, Type[] isExternalInitType)
        {
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.DeclaredOnly);

            foreach (var property in properties)
            {
                if (property.SetMethod == default
                    || property.SetMethod.ReturnParameter == null
                    || !property.SetMethod.ReturnParameter.GetRequiredCustomModifiers().Any(isExternalInitType.Contains)
                    || !property.SetMethod.IsPrivate)
                {
                    yield return new InvalidOperationException($"Property {property.ReflectedType.FullName}.{property.Name} should have an private init initializer");
                }
            }
        }
    }
}