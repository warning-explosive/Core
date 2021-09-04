namespace SpaceEngineers.Core.CompositionRoot.Api.Abstractions.Container
{
    using System;
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// Abstraction for dependency container that supports scopes
    /// </summary>
    public interface IScopedContainer : IResolvable
    {
        /// <summary>
        /// Open specified scope
        /// </summary>
        /// <returns>Scope cleanup</returns>
        IDisposable OpenScope();

        #if NETSTANDARD2_1

        /// <summary>
        /// Open specified scope
        /// </summary>
        /// <returns>Scope cleanup</returns>
        IAsyncDisposable OpenScopeAsync();

        #endif
    }
}