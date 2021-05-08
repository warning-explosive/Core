namespace SpaceEngineers.Core.CrossCuttingConcerns.Internals
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Api.Abstractions;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;

    [Component(EnLifestyle.Singleton)]
    internal class ObjectBuilder<T> : IObjectBuilder<T>
        where T : class
    {
        private readonly IDependencyContainer _dependencyContainer;

        public ObjectBuilder(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public T Build(IDictionary<string, object>? values = null)
        {
            values = values?.ToDictionary(it => it.Key, it => it.Value, StringComparer.OrdinalIgnoreCase)
                     ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            // 1. find .cctor (should have public constructor .cctor)
            var cctor = typeof(T)
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
                .EnsureNotNull($"{typeof(T).FullName} should have the default public constructor or a constructor that takes additional parameters");

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
            var instance = (T)cctor.Invoke(cctorArguments);

            // 4. fill instance with the leftover properties
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);

            foreach (var property in properties)
            {
                if (values.Remove(property.Name, out var value))
                {
                    property.SetValue(instance, ConvertTo(value, property.PropertyType));
                }
            }

            if (values.Any())
            {
                throw new InvalidOperationException($"Don't specify excessive properties: {values.Keys.ToString(", ")}");
            }

            return instance;
        }

        private object? ConvertTo(object? value, Type targetType)
        {
            if (value == null)
            {
                return null;
            }

            return ExecutionExtensions
                .Try(() => Convert(value, targetType))
                .Catch<Exception>()
                .Invoke(convertEx =>
                {
                    return ExecutionExtensions
                        .Try(() => Cast(value, targetType))
                        .Catch<Exception>()
                        .Invoke(castEx =>
                        {
                            return ExecutionExtensions
                                .Try(() => Transform(value, targetType))
                                .Catch<Exception>()
                                .Invoke(transformEx
                                    => throw new AggregateException($"Unable to convert value {value} from {value.GetType().FullName} to {targetType.FullName}", convertEx, castEx, transformEx));
                        });
                });
        }

        private static object? Convert(object value, Type targetType)
        {
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

            return System.Convert.ChangeType(value, targetType);
        }

        private static object? Cast(object value, Type targetType)
        {
            var constant = Expression.Constant(value);
            var convert = Expression.ConvertChecked(constant, targetType);
            return Expression.Lambda(convert).Compile().DynamicInvoke();
        }

        private object? Transform(object value, Type targetType)
        {
            return typeof(ObjectBuilder<T>)
                .CallMethod(nameof(Transform))
                .WithTypeArgument(value.GetType())
                .WithTypeArgument(targetType)
                .WithArgument(value)
                .ForInstance(this)
                .Invoke();
        }

        private TTarget Transform<TSource, TTarget>(TSource value)
        {
            return _dependencyContainer
                .Resolve<IObjectTransformer<TSource, TTarget>>()
                .Transform(value);
        }
    }
}