namespace SpaceEngineers.Core.Utilities.Services.Interfaces
{
    using System;

    /// <summary>
    /// Service for safely invoke client actions and exception handling
    /// </summary>
    public interface IExceptionHandler : IResolvable
    {
        /// <summary>
        /// Safely invoke client action
        /// </summary>
        /// <param name="action">Client action</param>
        /// <param name="exceptionHandlerAction">Exception handler action</param>
        /// <param name="finallyAction">Finally action</param>
        void TryCatchFinally(Action action,
                             Action<Exception> exceptionHandlerAction,
                             Action finallyAction = null);

        /// <summary>
        /// Safely invoke client action
        /// </summary>
        /// <param name="action">Client action</param>
        /// <param name="exceptionHandlerAction">Exception handler action</param>
        /// <param name="finallyAction">Finally action</param>
        /// <typeparam name="TReturn">Return-value type argument</typeparam>
        TReturn TryCatchFinally<TReturn>(Func<TReturn> action,
                                         Action<Exception> exceptionHandlerAction,
                                         Action finallyAction = null);
    }
}