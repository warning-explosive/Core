namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using Exceptions;

    /// <summary>
    /// Exception extension methods
    /// </summary>
    public static class ExceptionExtensions
    {
        private static readonly Type[] ExceptionTypesForSkip =
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
        /// Throw if input object is null
        /// </summary>
        /// <param name="input">input</param>
        /// <param name="message">Exception message</param>
        /// <typeparam name="TExpected">input type-argument</typeparam>
        /// <returns>Not null input or exception</returns>
        /// <exception cref="InvalidOperationException">Throws if input is null</exception>
        /// <exception cref="TypeMismatchException">Throws if TExpected type mismatched</exception>
        [return: NotNull]
        public static TExpected ExtractNotNullableSafely<TExpected>([AllowNull] this object input, string? message = null)
        {
            if (input == null)
            {
                throw new InvalidOperationException(message ?? $"{nameof(input)} is null");
            }

            if (input is TExpected expected)
            {
                return expected;
            }

            throw new TypeMismatchException(input.GetType(), typeof(TExpected));
        }

        /// <summary>
        /// Throw if input object is null
        /// </summary>
        /// <param name="input">input</param>
        /// <param name="message">Exception message</param>
        /// <typeparam name="TExpected">input type-argument</typeparam>
        /// <returns>Not null input or exception</returns>
        /// <exception cref="InvalidOperationException">Throws if input is null</exception>
        [return: NotNull]
        public static TExpected ExtractNotNullableSafely<TExpected>([AllowNull] this TExpected input, string? message = null)
        {
            if (input == null)
            {
                throw new InvalidOperationException(message ?? $"{nameof(input)} is null");
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
        /// <returns>Return value of the function or not handled exception</returns>
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
        /// <returns>Return value of the function or not handled exception</returns>
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
            return !ExceptionTypesForSkip.Contains(exception.RealException().GetType());
        }
    }
}