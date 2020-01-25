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
        public static TExpected ExtractNotNullable<TExpected>([AllowNull] this object input, string? message = null)
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
        public static TExpected ExtractNotNullable<TExpected>([AllowNull] this TExpected input, string? message = null)
        {
            if (input == null)
            {
                throw new ArgumentNullException(message ?? $"{nameof(input)} is null");
            }

            return input;
        }
    }
}