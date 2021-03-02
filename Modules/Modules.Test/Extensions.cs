namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoWiring.Api.Abstractions;

    internal static class Extensions
    {
        internal static IEnumerable<Type> ExtractDecorators<TService>(this TService service)
            where TService : class
        {
            while (service is IDecorator<TService> decorator)
            {
                yield return decorator.GetType();
                service = decorator.Decoratee;
            }

            yield return service.GetType();
        }

        internal static IEnumerable<Type> ShowTypes(this IEnumerable<Type> types, string tag, Action<string> show)
        {
            show(tag);
            return types.Select(type =>
                                {
                                    show(type.FullName ?? "null");
                                    return type;
                                });
        }
    }
}