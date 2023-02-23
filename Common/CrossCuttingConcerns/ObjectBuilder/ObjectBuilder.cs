namespace SpaceEngineers.Core.CrossCuttingConcerns.ObjectBuilder
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot;

    [Component(EnLifestyle.Singleton)]
    internal class ObjectBuilder : IObjectBuilder,
                                   IResolvable<IObjectBuilder>
    {
        private readonly IDependencyContainer _dependencyContainer;

        public ObjectBuilder(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public object? Build(Type type, IDictionary<string, object?>? values = null)
        {
            if (values?.Count == 1 && type.IsPrimitive())
            {
                return ConvertTo(values.Single().Value, type);
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
                .EnsureNotNull($"{type.FullName} should have the default public constructor or a constructor that takes additional parameters");

            // 2. convert .cctor parameters
            var cctorParameters = cctor.GetParameters();
            var cctorArguments = new object?[cctorParameters.Length];
            for (var i = 0; i < cctorParameters.Length; i++)
            {
                var cctorParameter = cctorParameters[i];

                if (values.Remove(cctorParameter.Name, out var argument))
                {
                    cctorArguments[i] = ConvertTo(argument, cctorParameter.ParameterType);
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
                        property.SetValue(instance, ConvertTo(value, property.PropertyType));
                    }
                }

                curr = curr.BaseType;
            }

            if (values.Any())
            {
                throw new InvalidOperationException($"Couldn't set value: {values.ToString(", ", pair => $"[{pair.Key}] - {pair.Value}")}");
            }
        }

        private object? ConvertTo(object? value, Type targetType)
        {
            if (value == null)
            {
                return null;
            }

            return ExecutionExtensions
                .Try(Convert, (value, targetType))
                .Catch<Exception>()
                .Invoke(convertEx =>
                {
                    return ExecutionExtensions
                        .Try(Cast, (value, targetType))
                        .Catch<Exception>()
                        .Invoke(castEx =>
                        {
                            return ExecutionExtensions
                                .Try(Transform, (value, targetType))
                                .Catch<Exception>()
                                .Invoke(transformEx => throw new AggregateException($"Unable to convert value {value} from {value.GetType().FullName} to {targetType.FullName}", convertEx, castEx, transformEx));
                        });
                });
        }

        private static object? Convert((object, Type) state)
        {
            var (value, targetType) = state;

            var fromConverter = TypeDescriptor.GetConverter(targetType);

            if (fromConverter.CanConvertFrom(value.GetType()))
            {
                return fromConverter.ConvertFrom(value);
            }

            var toConverter = TypeDescriptor.GetConverter(value.GetType());

            if (toConverter.CanConvertTo(targetType))
            {
                return toConverter.ConvertTo(value, targetType);
            }

            return System.Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }

        private static object? Cast((object, Type) state)
        {
            var (value, targetType) = state;

            var constant = Expression.Constant(value);
            var convert = Expression.ConvertChecked(constant, targetType);
            return Expression.Lambda(convert).Compile().DynamicInvoke();
        }

        private object? Transform((object, Type) state)
        {
            var (value, targetType) = state;

            return _dependencyContainer
                .ResolveGeneric(typeof(IObjectTransformer<,>), value.GetType(), targetType)
                .CallMethod(nameof(IObjectTransformer<object, object>.Transform))
                .WithArgument(value)
                .Invoke();
        }
    }
}