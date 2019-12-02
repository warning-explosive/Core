namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Exception extension methods
    /// </summary>
    public static class ExceptionExtensions
    {
        private static readonly Type[] _exceptionTypesForSkip =
        {
            typeof(StackOverflowException),
            typeof(OutOfMemoryException),
            typeof(OperationCanceledException),
        };

        /// <summary>
        /// Unwrap TargetInvocationException
        /// </summary>
        /// <param name="exception">exception</param>
        /// <returns>Real exception hidden beside TargetInvocationException</returns>
        public static Exception RealException(this Exception exception)
        {
            while (exception is TargetInvocationException tex)
            {
                exception = tex.InnerException;
            }

            return exception;
        }

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
            try
            {
                action.Invoke();
            }
            catch (Exception ex) when (CanBeCatched(ex))
            {
                exceptionHandler.Invoke(ex.RealException());
            }
            finally
            {
                finallyAction?.Invoke();
            }
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
            TResult? result = default;
            
            try
            {
                result = func.Invoke();
            }
            catch (Exception ex) when (CanBeCatched(ex))
            {
                exceptionHandler.Invoke(ex.RealException());
            }
            finally
            {
                finallyAction?.Invoke();
            }

            return result;
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
            TResult? result = default;
            
            try
            {
                result = func.Invoke();
            }
            catch (Exception ex) when (CanBeCatched(ex))
            {
                exceptionHandler.Invoke(ex.RealException());
            }
            finally
            {
                finallyAction?.Invoke();
            }

            return result;
        }

        private static bool CanBeCatched(Exception exception)
        {
            return !_exceptionTypesForSkip.Contains(exception.RealException().GetType());
        }
    }
}