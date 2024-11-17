namespace SpaceEngineers.Core.DependencyInjection.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Basics;

public static class DependenciesExtensions
{
    private static readonly Lazy<IReadOnlyDictionary<Type, IReadOnlyCollection<Type>>> BeforeAttributeDependencies
        = new Lazy<IReadOnlyDictionary<Type, IReadOnlyCollection<Type>>>(InitializeBeforeAttributeDependencies, LazyThreadSafetyMode.ExecutionAndPublication);

    private static readonly Lazy<IReadOnlyDictionary<Type, IReadOnlyCollection<Type>>> BeforeAfterAttributesDependencies
        = new Lazy<IReadOnlyDictionary<Type, IReadOnlyCollection<Type>>>(InitializeBeforeAfterAttributesDependencies, LazyThreadSafetyMode.ExecutionAndPublication);

    public static IOrderedEnumerable<Type> OrderByBeforeAfterAttributes(this IEnumerable<Type> source)
    {
        return source.OrderByBeforeAfterAttributes(t => t.GenericTypeDefinitionOrSelf());
    }

    public static IOrderedEnumerable<T> OrderByBeforeAfterAttributes<T>(this IEnumerable<T> source, Func<T, Type> accessor)
    {
        return source.OrderBy(SortFunc);

        int SortFunc(T item)
        {
            var type = accessor(item);
            var dependencies = GetDependenciesByAttributes(type).ToList();

            var depth = 0;

            while (dependencies.Any())
            {
                if (dependencies.Contains(type))
                {
                    throw new InvalidOperationException($"{type} has cycle dependency");
                }

                ++depth;

                dependencies = dependencies.SelectMany(GetDependenciesByAttributes).ToList();
            }

            return depth;
        }

        IEnumerable<Type> GetDependenciesByAttributes(Type type)
        {
            return BeforeAfterAttributesDependencies.Value.TryGetValue(type, out var dependencies)
                ? dependencies
                : Enumerable.Empty<Type>();
        }
    }

    private static IReadOnlyDictionary<Type, IReadOnlyCollection<Type>> InitializeBeforeAfterAttributesDependencies()
    {
        return TypeExtensions
            .AllTypes()
            .ToDictionary(type => type, ExtractDependencies);

        static IReadOnlyCollection<Type> ExtractDependencies(Type type)
        {
            var byAfterAttribute = type
                .GetCustomAttribute<AfterAttribute>()
                ?.Types ?? Enumerable.Empty<Type>();

            var byBeforeAttribute = BeforeAttributeDependencies.Value.TryGetValue(type, out var value)
                ? value
                : Enumerable.Empty<Type>();

            return byAfterAttribute.Concat(byBeforeAttribute).ToList();
        }
    }

    private static IReadOnlyDictionary<Type, IReadOnlyCollection<Type>> InitializeBeforeAttributeDependencies()
    {
        return TypeExtensions
            .AllTypes()
            .Select(type =>
            {
                var attribute = type.GetCustomAttribute<BeforeAttribute>();
                return (type, attribute);
            })
            .Where(pair => pair.attribute != null)
            .SelectMany(pair => pair.attribute.Types.Select(before => (before, pair.type)))
            .GroupBy(pair => pair.before, pair => pair.type)
            .ToDictionary(grp => grp.Key, grp => grp.ToList() as IReadOnlyCollection<Type>);
    }
}