namespace SpaceEngineers.Core.CrossCuttingConcerns.ObjectBuilder
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;

    [SuppressMessage("Analysis", "CA1031", Justification = "desired behavior")]
    [Component(EnLifestyle.Singleton)]
    internal class ObjectBuilder : IObjectBuilder,
                                   IResolvable<IObjectBuilder>
    {
        private static readonly ITypeDescriptorContext Context = new ObjectBuilderTypeDescriptorContext();

        static ObjectBuilder()
        {
            TypeDescriptor.AddAttributes(
                typeof(DBNull),
                new TypeConverterAttribute(typeof(DbNullTypeConverter)));

            TypeDescriptor.AddAttributes(
                TypeExtensions.FindType("System.Private.CoreLib System.DateOnly"),
                new TypeConverterAttribute(typeof(DateOnlyTypeConverter)));

            TypeDescriptor.AddAttributes(
                TypeExtensions.FindType("System.Private.CoreLib System.TimeOnly"),
                new TypeConverterAttribute(typeof(TimeOnlyTypeConverter)));

            TypeDescriptor.AddAttributes(
                typeof(Type),
                new TypeConverterAttribute(typeof(TypeNodeTypeConverter)));
        }

        public object? Build(Type type, IDictionary<string, object?>? values = null)
        {
            if (values != null
                && values.Count == 1
                && TryConvertTo(type, values.Single().Value, out var value))
            {
                return value;
            }

            values = values?.ToDictionary(it => it.Key, it => it.Value, StringComparer.OrdinalIgnoreCase)
                     ?? new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            // 1. find .cctor (should have public constructor .cctor)
            var cctor = type
                            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance)
                            .Select(info =>
                            {
                                var constructorInfo = info;
                                var parameters = info.GetParameters();
                                return (constructorInfo, parameters);
                            })
                            .Where(info => info.parameters.All(parameter => values.ContainsKey(parameter.Name)))
                            .OrderByDescending(info => info.parameters.Length)
                            .Select(info => info.constructorInfo)
                            .FirstOrDefault()
                        ?? throw new InvalidOperationException($"{type.FullName} should have the default public constructor or a constructor that takes additional parameters");

            // 2. convert .cctor parameters
            var cctorParameters = cctor.GetParameters();
            var cctorArguments = new object?[cctorParameters.Length];
            for (var i = 0; i < cctorParameters.Length; i++)
            {
                var cctorParameter = cctorParameters[i];

                if (values.Remove(cctorParameter.Name, out var argument))
                {
                    cctorArguments[i] = ConvertTo(cctorParameter.ParameterType, argument);
                }
                else
                {
                    throw new InvalidOperationException($"Missing constructor argument {cctorParameter.ParameterType} {cctorParameter.Name} at position {i}");
                }
            }

            // 3. build instance
            var instance = cctor.Invoke(cctorArguments);

            // 4. fill instance with the leftover properties
            Fill(type, instance, values);

            return instance;
        }

        public void Fill(Type type, object instance, IDictionary<string, object?> values)
        {
            var curr = type;

            while (curr != null)
            {
                var properties = curr
                   .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty)
                   .Where(property => property.SetIsAccessible());

                foreach (var property in properties)
                {
                    if (values.Remove(property.Name, out var value))
                    {
                        property.SetValue(instance, ConvertTo(property.PropertyType, value));
                    }
                }

                curr = curr.BaseType;
            }

            if (values.Any())
            {
                throw new InvalidOperationException($"Couldn't set value: {values.ToString(", ", pair => $"[{pair.Key}] - {pair.Value}")}");
            }
        }

        private static object? ConvertTo(
            Type type,
            object? value)
        {
            return TryConvertTo(type, value, out var result)
                ? result
                : throw new InvalidOperationException($"Unable to convert value {value} from {value.GetType().FullName} to {type.FullName}");
        }

        private static bool TryConvertTo(
            Type type,
            object? value,
            out object? result)
        {
            if (value == null)
            {
                result = null;
                return true;
            }

            if (TryMatch(type, value, out result)
                || TryConvert(type, value, out result)
                || TryCast(type, value, out result))
            {
                return true;
            }

            result = null;
            return false;
        }

        private static bool TryMatch(
            Type type,
            object value,
            out object? result)
        {
            if (value.IsInstanceOfType(type))
            {
                result = value;
                return true;
            }

            result = null;
            return false;
        }

        private static bool TryConvert(
            Type type,
            object value,
            out object? result)
        {
            try
            {
                var fromConverter = TypeDescriptor.GetConverter(type);

                if (fromConverter.CanConvertFrom(Context, value.GetType()))
                {
                    result = fromConverter.ConvertFrom(Context, CultureInfo.InvariantCulture, value);
                    return true;
                }

                var toConverter = TypeDescriptor.GetConverter(value.GetType());

                if (toConverter.CanConvertTo(Context, type))
                {
                    result = toConverter.ConvertTo(Context, CultureInfo.InvariantCulture, value, type);
                    return true;
                }

                result = Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
                return true;
            }
            catch (Exception ex)
            {
                _ = ex;
            }

            result = null;
            return false;
        }

        private static bool TryCast(
            Type type,
            object value,
            out object? result)
        {
            try
            {
                var constant = Expression.Constant(value);
                var convert = Expression.ConvertChecked(constant, type);
                result = Expression.Lambda(convert).Compile().DynamicInvoke();
                return true;
            }
            catch (Exception ex)
            {
                _ = ex;
            }

            result = null;
            return false;
        }
    }
}