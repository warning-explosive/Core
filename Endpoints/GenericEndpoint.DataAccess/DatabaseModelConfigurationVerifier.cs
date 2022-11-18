namespace SpaceEngineers.Core.GenericEndpoint.DataAccess
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using Basics;
    using CompositionRoot;
    using CompositionRoot.Registration;
    using CompositionRoot.Verifiers;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.DataAccess.Api.Model;
    using SpaceEngineers.Core.DataAccess.Api.Sql;
    using SpaceEngineers.Core.DataAccess.Api.Sql.Attributes;

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
                .Where(type => typeof(IDatabaseEntity).IsAssignableFrom(type)
                               && type.IsConcreteType())
                .SelectMany(type => VerifyDatabaseEntity(type, typeof(BaseDatabaseEntity<>)))
                .Each(exception => throw exception.Rethrow());

            if (TypeExtensions.TryFindType("SpaceEngineers.Core.DataAccess.Api.Sql SpaceEngineers.Core.DataAccess.Api.Sql.ISqlView`1", out var sqlView))
            {
                var baseSqlView = TypeExtensions.FindType("SpaceEngineers.Core.DataAccess.Api.Sql SpaceEngineers.Core.DataAccess.Api.Sql.BaseSqlView`1");

                _typeProvider
                .OurTypes
                .Where(type => type.IsSubclassOfOpenGeneric(sqlView)
                            && type.IsConcreteType())
                .SelectMany(type => VerifyDatabaseEntity(type, baseSqlView))
                .Each(exception => throw exception.Rethrow());
            }

            _typeProvider
                .OurTypes
                .Where(type => typeof(IInlinedObject).IsAssignableFrom(type)
                               && type.IsConcreteType())
                .SelectMany(VerifyInlinedObject)
                .Each(exception => throw exception.Rethrow());
        }

        private IEnumerable<Exception> VerifyDatabaseEntity(Type databaseEntity, Type baseOpenGenericType)
        {
            if (TypeHasWrongConstructor(databaseEntity, out var constructorException))
            {
                yield return constructorException;
            }

            if (TypeHasWrongModifier(databaseEntity, out var modifierException))
            {
                yield return modifierException;
            }

            if (TypeDoesNotInheritFrom(databaseEntity, baseOpenGenericType, out var inheritanceException))
            {
                yield return inheritanceException;
            }

            foreach (var setterException in DatabaseEntityHasMissingPropertySetter(databaseEntity))
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

            foreach (var setterException in DatabaseInlinedObjectHasMissingPropertyInitializer(inlinedObject))
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
            if (type.IsRecord())
            {
                exception = default;
                return false;
            }

            exception = new InvalidOperationException($"Type {type} should be defined as record");
            return true;
        }

        private static bool TypeDoesNotInheritFrom(
            Type type,
            Type baseType,
            [NotNullWhen(true)] out Exception? exception)
        {
            var inherits = (baseType.IsGenericType && type.IsSubclassOfOpenGeneric(baseType))
                        || baseType.IsAssignableFrom(type);

            if (!inherits)
            {
                exception = new InvalidOperationException($"Type {type} should implement {baseType}");
                return true;
            }

            exception = default;
            return false;
        }

        private static IEnumerable<Exception> DatabaseEntityHasMissingPropertySetter(Type type)
        {
            var properties = type
               .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.DeclaredOnly)
               .Where(property => !property.Name.Equals("EqualityContract", StringComparison.OrdinalIgnoreCase))
               .Where(property => !property.SetIsAccessible() || property.HasInitializer());

            foreach (var property in properties)
            {
                yield return new InvalidOperationException($"Property {property.ReflectedType.FullName}.{property.Name} should have public setter so as to be mutable and deserializable");
            }
        }

        private static IEnumerable<Exception> DatabaseInlinedObjectHasMissingPropertyInitializer(Type type)
        {
            var properties = type
               .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.DeclaredOnly)
               .Where(property => !property.Name.Equals("EqualityContract", StringComparison.OrdinalIgnoreCase))
               .Where(property => !(property.HasInitializer() && property.SetIsAccessible()));

            foreach (var property in properties)
            {
                yield return new InvalidOperationException($"Property {property.ReflectedType.FullName}.{property.Name} should have public initializer (init modifier) so as to be immutable and deserializable");
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