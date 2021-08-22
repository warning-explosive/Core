namespace SpaceEngineers.Core.Test.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// System.Type extensions for test project
    /// </summary>
    public static class SystemTypeTestExtensions
    {
        /// <summary> Show system types </summary>
        /// <param name="source">Source</param>
        /// <param name="tag">Tag</param>
        /// <param name="show">Show action</param>
        /// <returns>Types projection</returns>
        public static IEnumerable<Type> ShowTypes(
            this IEnumerable<Type> source,
            string tag,
            Action<string> show)
        {
            show(tag);
            return source.Select(type =>
            {
                show(type.FullName ?? "null");
                return type;
            });
        }

        /// <summary> Extract decorators from source object </summary>
        /// <param name="service">Service implementation</param>
        /// <typeparam name="TService">TService type argument</typeparam>
        /// <returns>Applied decorators</returns>
        public static IEnumerable<Type> UnwrapDecorators<TService>(this TService service)
            where TService : class
        {
            while (service is IDecorator<TService> decorator)
            {
                yield return decorator.GetType();
                service = decorator.Decoratee;
            }

            yield return service.GetType();
        }
    }
}