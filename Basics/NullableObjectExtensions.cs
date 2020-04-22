namespace SpaceEngineers.Core.Basics
{
    using System;
    using Exceptions;

    /// <summary>
    /// Extension for work with nullable objects and types
    /// </summary>
    public static class NullableObjectExtensions
    {
        /// <summary>
        /// Throw if input object is null
        /// </summary>
        /// <param name="input">input</param>
        /// <typeparam name="TExpected">input type-argument</typeparam>
        /// <returns>Not null input or exception</returns>
        /// <exception cref="TypeMismatchException">Throws if TExpected type mismatched</exception>
        public static TExpected ExtractType<TExpected>(this object input)
        {
            return (TExpected)input;
        }

        /// <summary>
        /// Throw if input object is null
        /// </summary>
        /// <param name="input">input</param>
        /// <param name="message">Exception message</param>
        /// <typeparam name="TExpected">input type-argument</typeparam>
        /// <returns>Not null input or exception</returns>
        /// <exception cref="TypeMismatchException">Throws if TExpected type mismatched</exception>
        /// <exception cref="InvalidOperationException">Throws if input is null</exception>
        public static TExpected TryExtractFromNullable<TExpected>(this object? input, string? message = null)
        {
            return TryExtractFromNullable<TExpected>(input, () => new InvalidOperationException(message ?? $"{nameof(input)} is null"));
        }

        /// <summary>
        /// Throw if input object is null
        /// </summary>
        /// <param name="input">input</param>
        /// <param name="exceptionFactory">Custom exception factory method</param>
        /// <typeparam name="TExpected">input type-argument</typeparam>
        /// <returns>Not null input or exception</returns>
        /// <exception cref="TypeMismatchException">Throws if TExpected type mismatched</exception>
        /// <exception cref="Exception">Throws if input is null</exception>
        public static TExpected TryExtractFromNullable<TExpected>(this object? input, Func<Exception> exceptionFactory)
        {
            if (input == null)
            {
                throw exceptionFactory();
            }

            if (input is TExpected expected)
            {
                return expected;
            }

            throw new TypeMismatchException(typeof(TExpected), input.GetType());
        }

        /// <summary>
        /// Throw if input object is null
        /// </summary>
        /// <param name="input">input</param>
        /// <param name="message">Exception message</param>
        /// <typeparam name="TExpected">input type-argument</typeparam>
        /// <returns>Not null input or exception</returns>
        /// <exception cref="ArgumentNullException">Throws if input is null</exception>
        public static TExpected TryExtractFromNullable<TExpected>(this TExpected? input, string? message = null)
            where TExpected : class
        {
            return TryExtractFromNullable(input, () => new ArgumentNullException(message ?? $"{nameof(input)} is null"));
        }

        /// <summary>
        /// Throw if input object is null
        /// </summary>
        /// <param name="input">input</param>
        /// <param name="exceptionFactory">Custom exception factory method</param>
        /// <typeparam name="TExpected">input type-argument</typeparam>
        /// <returns>Not null input or exception</returns>
        /// <exception cref="Exception">Throws if input is null</exception>
        public static TExpected TryExtractFromNullable<TExpected>(this TExpected? input, Func<Exception> exceptionFactory)
            where TExpected : class
        {
            if (input == null)
            {
                throw exceptionFactory();
            }

            return input;
        }
    }
}