namespace SpaceEngineers.Core.CompositionRoot.Extensions
{
    using System;
    using Abstractions;

    /// <summary>
    /// Exception extension methods
    /// </summary>
    public static class ExceptionExtensions
    {
        private static readonly IExceptionHandler _exceptionHandler = DependencyContainer.Resolve<IExceptionHandler>();

        /// <summary>
        /// Throw if input class is null
        /// </summary>
        /// <param name="input">input</param>
        /// <param name="message">Exception message</param>
        /// <typeparam name="T">input type-argument</typeparam>
        /// <returns>input</returns>
        /// <exception cref="ArgumentNullException">Throws if input is null</exception>
        public static T ThrowIfNull<T>(this T? input, string? message = null)
            where T : class
        {
            if (input == null)
            {
                throw new ArgumentNullException(message ?? nameof(input));
            }

            return input;
        }

        /// <summary>
        /// Safely invoke client action
        /// </summary>
        /// <param name="action">Client action</param>
        /// <param name="exceptionHandler">Exception handler action</param>
        /// <param name="finallyAction">Finally action</param>
        public static void HandleException(this Action action,
                                           Action<Exception> exceptionHandler,
                                           Action? finallyAction = null)
        {
            _exceptionHandler.TryCatchFinally(action, exceptionHandler, finallyAction);
        }

        /// <summary>
        /// Safely invoke client action with class result
        /// </summary>
        /// <param name="func">Client function</param>
        /// <param name="exceptionHandler">Exception handler action</param>
        /// <param name="finallyAction">Finally action</param>
        /// <typeparam name="TResult">Return-value type argument</typeparam>
        public static TResult? HandleException<TResult>(this Func<TResult?> func,
                                                        Action<Exception> exceptionHandler,
                                                        Action? finallyAction = null)
            where TResult : class
        {
            return _exceptionHandler.TryCatchFinally(func, exceptionHandler, finallyAction);
        }
        
        /// <summary>
        /// Safely invoke client action with struct result
        /// </summary>
        /// <param name="func">Client function</param>
        /// <param name="exceptionHandler">Exception handler action</param>
        /// <param name="finallyAction">Finally action</param>
        /// <typeparam name="TResult">Return-value type argument</typeparam>
        public static TResult? HandleException<TResult>(this Func<TResult?> func,
                                                        Action<Exception> exceptionHandler,
                                                        Action? finallyAction = null)
            where TResult : struct
        {
            return _exceptionHandler.TryCatchFinally(func, exceptionHandler, finallyAction);
        }
    }
}