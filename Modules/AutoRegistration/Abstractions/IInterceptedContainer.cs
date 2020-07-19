namespace SpaceEngineers.Core.AutoRegistration.Abstractions
{
    using System;
    using AutoWiringApi.Abstractions;

    /// <summary>
    /// Abstraction for container that can be intercepted
    /// </summary>
    public interface IInterceptedContainer
    {
        /// <summary>
        /// Apply unregistered decorator to resolution phase for specified service
        /// Decorator will be applied after container locking and after all registered decorators
        /// </summary>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <typeparam name="TDecorator">TDecorator type-argument</typeparam>
        /// <returns>Scope cleanup</returns>
        IDisposable ApplyDecorator<TService, TDecorator>()
            where TService : class
            where TDecorator : TService, IDecorator<TService>;
    }
}