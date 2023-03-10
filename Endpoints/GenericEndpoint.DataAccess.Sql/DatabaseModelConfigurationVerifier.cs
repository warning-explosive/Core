namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Sql
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Basics;
    using CompositionRoot;
    using CompositionRoot.Registration;
    using CompositionRoot.Verifiers;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model.Attributes;

    [Component(EnLifestyle.Singleton)]
    internal class DataAccessConfigurationVerifier : IConfigurationVerifier,
                                                     ICollectionResolvable<IConfigurationVerifier>
    {
        private readonly ITypeProvider _typeProvider;
        private readonly IModelProvider _modelProvider;
        private readonly IConstructorResolutionBehavior _constructorResolutionBehavior;

        public DataAccessConfigurationVerifier(
            ITypeProvider typeProvider,
            IModelProvider modelProvider,
            IConstructorResolutionBehavior constructorResolutionBehavior)
        {
            _typeProvider = typeProvider;
            _modelProvider = modelProvider;
            _constructorResolutionBehavior = constructorResolutionBehavior;
        }

        public void Verify()
        {
            var exceptions = new List<Exception>();

            var databaseEntities = _typeProvider
                .OurTypes
                .Where(type => type.IsDatabaseEntity() && type.IsConcreteType());

            VerifyDatabaseEntities(databaseEntities, typeof(BaseDatabaseEntity<>), exceptions);

            var sqlViews = _typeProvider
                .OurTypes
                .Where(type => type.IsSqlView() && type.IsConcreteType());

            VerifyDatabaseEntities(sqlViews, typeof(BaseSqlView<>), exceptions);

            var inlinedObjects = _typeProvider
                .OurTypes
                .Where(type => type.IsInlinedObject() && type.IsConcreteType());

            VerifyInlinedObjects(inlinedObjects, exceptions);

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }

        private void VerifyDatabaseEntities(
            IEnumerable<Type> databaseEntities,
            Type baseType,
            ICollection<Exception> exceptions)
        {
            foreach (var databaseEntity in databaseEntities)
            {
                VerifyModifiers(databaseEntity, exceptions);
                VerifyConstructors(databaseEntity, exceptions);
                VerifyInheritance(databaseEntity, baseType, exceptions);
                VerifySchemaAttribute(databaseEntity, exceptions);
                VerifyMissingPropertySetter(databaseEntity, exceptions);
                VerifyForeignKeys(_modelProvider, databaseEntity, exceptions);
            }

            static void VerifyForeignKeys(
                IModelProvider modelProvider,
                Type type,
                ICollection<Exception> exceptions)
            {
                var properties = modelProvider
                    .Columns(type)
                    .Where(column => column.IsRelation)
                    .Select(column => column.Relation.Property.Declared)
                    .Where(column => !column.HasAttribute<ForeignKeyAttribute>());

                foreach (var property in properties)
                {
                    exceptions.Add(new InvalidOperationException($"Property {property.ReflectedType.FullName}.{property.Name} should be marked by {nameof(ForeignKeyAttribute)}"));
                }
            }

            static void VerifyInheritance(
                Type type,
                Type baseType,
                ICollection<Exception> exceptions)
            {
                if (!type.IsSubclassOfOpenGeneric(baseType))
                {
                    exceptions.Add(new InvalidOperationException($"Type {type} should implement {typeof(BaseDatabaseEntity<>)}"));
                }
            }

            static void VerifyMissingPropertySetter(
                Type type,
                ICollection<Exception> exceptions)
            {
                var properties = type
                    .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.DeclaredOnly)
                    .Where(property => !property.IsEqualityContract())
                    .Where(property => !(!property.HasInitializer() && property.SetIsAccessible()));

                foreach (var property in properties)
                {
                    exceptions.Add(new InvalidOperationException($"Property {property.ReflectedType.FullName}.{property.Name} should have public setter so as to be mutable and deserializable"));
                }
            }

            static void VerifySchemaAttribute(
                Type type,
                ICollection<Exception> exceptions)
            {
                if (!type.HasAttribute<SchemaAttribute>())
                {
                    exceptions.Add(new InvalidOperationException($"Type {type} should be marked by {typeof(SchemaAttribute).FullName}"));
                }
            }
        }

        private void VerifyInlinedObjects(
            IEnumerable<Type> inlinedObjects,
            ICollection<Exception> exceptions)
        {
            foreach (var inlinedObject in inlinedObjects)
            {
                VerifyModifiers(inlinedObject, exceptions);
                VerifyConstructors(inlinedObject, exceptions);
                VerifyMissingPropertyInitializer(inlinedObject, exceptions);
            }

            static void VerifyMissingPropertyInitializer(
                Type inlinedObject,
                ICollection<Exception> exceptions)
            {
                var properties = inlinedObject
                    .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.DeclaredOnly)
                    .Where(property => !property.IsEqualityContract())
                    .Where(property => !(property.HasInitializer() && property.SetIsAccessible()));

                foreach (var property in properties)
                {
                    exceptions.Add(new InvalidOperationException($"Property {property.ReflectedType.FullName}.{property.Name} should have public initializer (init modifier) so as to be immutable and deserializable"));
                }
            }
        }

        private static void VerifyModifiers(
            Type type,
            ICollection<Exception> exceptions)
        {
            if (!type.IsRecord())
            {
                exceptions.Add(new InvalidOperationException($"Type {type} should be defined as record"));
            }
        }

        private void VerifyConstructors(
            Type type,
            ICollection<Exception> exceptions)
        {
            if (!_constructorResolutionBehavior.TryGetConstructor(type, out _))
            {
                exceptions.Add(new InvalidOperationException($"Type {type} should have one public constructor"));
            }
        }
    }
}