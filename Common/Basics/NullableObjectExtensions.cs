namespace SpaceEngineers.Core.Basics
{
    using Exceptions;

    /// <summary>
    /// Extension for work with nullable objects and types
    /// </summary>
    public static class NullableObjectExtensions
    {
        /// <summary>
        /// Converts input object to another type
        /// </summary>
        /// <param name="input">input</param>
        /// <typeparam name="TExpected">Output type-argument</typeparam>
        /// <returns>Converted output or exception</returns>
        /// <exception cref="TypeMismatchException">Throws if TExpected type is mismatched</exception>
        public static TExpected EnsureType<TExpected>(this object? input)
        {
            if (input is TExpected expected)
            {
                return expected;
            }

            throw new TypeMismatchException(typeof(TExpected), input.GetType());
        }
    }
}