namespace SpaceEngineers.Core.CompositionRoot.Api.Extensions
{
    using System;
    using System.Collections.Generic;
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// ComponentsExtensions
    /// </summary>
    public static class ComponentsExtensions
    {
        /// <summary>
        /// Flattens decorators and implementation objects from the source object tree
        /// </summary>
        /// <param name="service">Service implementation</param>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <returns>Unwrapped decorators and implementation</returns>
        public static IEnumerable<TService> FlattenDecoratedObject<TService>(this TService service)
            where TService : class
        {
            while (service is IDecorator<TService> decorator)
            {
                yield return service;
                service = decorator.Decoratee;
            }

            yield return service;
        }

        /// <summary>
        /// Flattens decorators and implementation types from the source object type
        /// </summary>
        /// <param name="service">Service implementation</param>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <returns>Unwrapped decorators and implementation</returns>
        public static IEnumerable<Type> FlattenDecoratedType<TService>(this TService service)
            where TService : class
        {
            while (service is IDecorator<TService> decorator)
            {
                yield return service.GetType();
                service = decorator.Decoratee;
            }

            yield return service.GetType();
        }
    }
}