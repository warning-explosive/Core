namespace SpaceEngineers.Core.Web.Api.Containers
{
    using System;
    using System.Linq;
    using System.Reflection;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;

    [Component(EnLifestyle.Singleton)]
    internal class DataContainersProvider : IDataContainersProvider
    {
        private const string UnknownTypeFormat = "Unknown data type: {0}";

        public ViewEntity ToViewEntity<TEntity>(TEntity entity)
        {
            var containers = entity
                .GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)
                .ToDictionary(
                    property => property.Name,
                    property => ValueToContainer(property.GetValue(entity), property.PropertyType),
                    StringComparer.OrdinalIgnoreCase);

            return new ViewEntity(containers);
        }

        private static IDataContainer ValueToContainer(object? value, Type type)
        {
            type = type.UnwrapTypeParameter(typeof(Nullable<>));

            if (type == typeof(string))
            {
                return new StringDataContainer(value as string);
            }

            if (type.IsNumeric())
            {
                return new NumericDataContainer(value as double?);
            }

            if (type == typeof(DateTime))
            {
                return new DateTimeDataContainer(value as DateTime?);
            }

            if (type == typeof(bool))
            {
                return new BooleanDataContainer(value as bool?);
            }

            return new StringDataContainer(UnknownTypeFormat.Format(type.FullName!));
        }
    }
}