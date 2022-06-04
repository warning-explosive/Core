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
        /// <param name="selector">Result selector</param>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <typeparam name="TResult">TResult type-argument</typeparam>
        /// <returns>Unwrapped decorators and implementation</returns>
        public static IEnumerable<TResult> FlattenDecoratedObject<TService, TResult>(
            this TService service,
            Func<object, TResult> selector)
            where TService : class
        {
            while (service is IDecorator<TService> decorator)
            {
                yield return selector(service);
                service = decorator.Decoratee;
            }

            yield return selector(service);
        }
    }
}