namespace SpaceEngineers.Core.GenericEndpoint.Abstractions
{
    using System;
    using System.Threading.Tasks;
    using AutoRegistration.Abstractions;

    /// <summary>
    /// Abstraction for physical endpoint instance
    /// </summary>
    public interface IEndpoint : IAsyncDisposable
    {
        /// <summary>
        /// Execute operation in scope of running endpoint instance
        /// </summary>
        /// <param name="worker">Worker</param>
        /// <returns>Running operation abstraction</returns>
        Task Execute(Func<IDependencyContainer, Task> worker);
    }
}