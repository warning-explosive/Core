namespace SpaceEngineers.Core.CrossCuttingConcerns
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;

    [Component(EnLifestyle.Singleton)]
    internal class ObjectBuilder<T> : IObjectBuilder<T>
    {
        public T Build(IDictionary<string, object> propertyValues)
        {
            // 1. find .cctor (should have 1 .cctor)
            var cctor = typeof(T)
                .GetConstructors()
                .InformativeSingle(_ => $"{typeof(T).FullName} should have one public constructor for all purposes");

            // 2. converts columns
            var properties = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty);

            var values = new Dictionary<string, (PropertyInfo PropertyInfo, object? PropertyValue)>(StringComparer.OrdinalIgnoreCase);

            foreach (var property in properties)
            {
                if (propertyValues.TryGetValue(property.Name, out var rawColumnValue))
                {
                    var columnValue = Convert(rawColumnValue, property.PropertyType);
                    values[property.Name] = (property, columnValue);
                }
            }

            // 3. build instance
            var cctorParameters = cctor.GetParameters();
            var args = new object?[cctorParameters.Length];

            for (var i = 0; i < cctorParameters.Length; i++)
            {
                var parameterName = cctorParameters[i].Name;
                var parameterType = cctorParameters[i].ParameterType;

                if (!values.Remove(parameterName, out var info)
                    || !parameterType.IsAssignableFrom(info.PropertyInfo.PropertyType))
                {
                    throw new InvalidOperationException($"Could not find constructor parameter: {parameterType.FullName} {parameterName}");
                }

                args[i] = info.PropertyValue;
            }

            var aggregate = (T)cctor.Invoke(args);

            // 4. fill instance with the leftover properties
            foreach (var value in values)
            {
                var (propertyInfo, propertyValue) = value.Value;
                propertyInfo.SetValue(aggregate, propertyValue);
            }

            return aggregate;
        }

        private static object? Convert(object? rawValue, Type targetType)
        {
            if (rawValue == null)
            {
                return null;
            }

            if (targetType.IsInstanceOfType(rawValue))
            {
                return rawValue;
            }

            /*
             * TODO: convert raw value;
             *  1. Try explicit or implicit cast
             *  2. Try converter
             *  ...
             *  N. throw exception
             */

            throw new NotImplementedException();
        }
    }
}