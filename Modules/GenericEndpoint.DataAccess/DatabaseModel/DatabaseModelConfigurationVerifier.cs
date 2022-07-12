﻿namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.DatabaseModel
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
            _typeProvider
                .OurTypes
                .Where(type => type.IsSubclassOfOpenGeneric(typeof(IDatabaseEntity<>))
                            && type.IsConcreteType())
                .SelectMany(VerifyDatabaseEntity)
                .Each(exception => throw exception.Rethrow());

            var sqlView = AssembliesExtensions.FindType("SpaceEngineers.Core.DataAccess.Orm.Sql.Views.ISqlView`1");

            if (sqlView != null)
            {
                _typeProvider
                .OurTypes
                .Where(type => type.IsSubclassOfOpenGeneric(sqlView)
                            && type.IsConcreteType())
                .SelectMany(VerifyDatabaseEntity)
                .Each(exception => throw exception.Rethrow());
            }

            _typeProvider
                .OurTypes
                .Where(type => typeof(IInlinedObject).IsAssignableFrom(type)
                               && type.IsConcreteType())
                .SelectMany(VerifyInlinedObject)
                .Each(exception => throw exception.Rethrow());
        }

        private IEnumerable<Exception> VerifyDatabaseEntity(Type databaseEntity)
        {
            if (TypeHasWrongConstructor(databaseEntity, out var constructorException))
            {
                yield return constructorException;
            }

            if (TypeHasWrongModifier(databaseEntity, out var modifierException))
            {
                yield return modifierException;
            }

            foreach (var setterException in DatabaseObjectHasMissingPropertySetter(databaseEntity))
            {
                yield return setterException;
            }

            if (TypeHasMissingSchemaAttribute(databaseEntity, out var missingSchemaAttributeException))
            {
                yield return missingSchemaAttributeException;
            }
        }

        private IEnumerable<Exception> VerifyInlinedObject(Type inlinedObject)
        {
            if (TypeHasWrongConstructor(inlinedObject, out var constructorException))
            {
                yield return constructorException;
            }

            if (TypeHasWrongModifier(inlinedObject, out var modifierException))
            {
                yield return modifierException;
            }

            foreach (var setterException in DatabaseObjectHasMissingPropertySetter(inlinedObject))
            {
                yield return setterException;
            }
        }

        private bool TypeHasWrongConstructor(
            Type type,
            [NotNullWhen(true)] out Exception? exception)
        {
            if (!_constructorResolutionBehavior.TryGetConstructor(type, out _))
            {
                exception = new InvalidOperationException($"Type {type} should have one public constructor");
                return true;
            }

            exception = default;
            return false;
        }

        private static bool TypeHasWrongModifier(
            Type type,
            [NotNullWhen(true)] out Exception? exception)
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

        private static IEnumerable<Exception> DatabaseObjectHasMissingPropertySetter(Type type)
        {
            var properties = type
               .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.DeclaredOnly)
               .Where(property => !property.Name.Equals("EqualityContract", StringComparison.OrdinalIgnoreCase))
               .Where(property => !property.SetIsAccessible());

            foreach (var property in properties)
            {
                yield return new InvalidOperationException($"Property {property.ReflectedType.FullName}.{property.Name} should have accessible setter or initializer (init modifier) so as to be deserializable");
            }
        }

        private static bool TypeHasMissingSchemaAttribute(
            Type type,
            [NotNullWhen(true)] out Exception? exception)
        {
            var schemaAttribute = type.GetAttribute<SchemaAttribute>();

            if (schemaAttribute == null)
            {
                exception = new InvalidOperationException($"Type {type} should be declared with {typeof(SchemaAttribute).FullName}");
                return true;
            }

            exception = default;
            return false;
        }
    }
}