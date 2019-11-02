namespace SpaceEngineers.Core.CompositionRoot.Extensions
{
    using System;
    using Abstractions;

    /// <summary>
    /// Service for safely invoke client actions and exception handling
    /// </summary>
    internal interface IExceptionHandler : IResolvable
    {
        /// <summary>
        /// Safely invoke client action
        /// </summary>
        /// <param name="action">Client action</param>
        /// <param name="exceptionHandler">Exception handler action</param>
        /// <param name="finallyAction">Finally action</param>
        void TryCatchFinally(Action action,
                             Action<Exception> exceptionHandler,
                             Action? finallyAction = null);

        /// <summary>
        /// Safely invoke client action with class result
        /// </summary>
        /// <param name="func">Client function</param>
        /// <param name="exceptionHandler">Exception handler action</param>
        /// <param name="finallyAction">Finally action</param>
        /// <typeparam name="TResult">Return-value type argument</typeparam>
        TResult? TryCatchFinally<TResult>(Func<TResult?> func,
                                          Action<Exception> exceptionHandler,
                                          Action? finallyAction = null)
            where TResult : class;

        /// <summary>
        /// Safely invoke client action with struct result
        /// </summary>
        /// <param name="func">Client function</param>
        /// <param name="exceptionHandler">Exception handler action</param>
        /// <param name="finallyAction">Finally action</param>
        /// <typeparam name="TResult">Return-value type argument</typeparam>
        TResult? TryCatchFinally<TResult>(Func<TResult?> func,
                                          Action<Exception> exceptionHandler,
                                          Action? finallyAction = null)
            where TResult : struct;
    }
}