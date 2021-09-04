namespace SpaceEngineers.Core.CrossCuttingConcerns.Api.Abstractions
{
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// IStringFormatter
    /// </summary>
    /// <typeparam name="T">T type-argument</typeparam>
    public interface IStringFormatter<T> : IResolvable
    {
        /// <summary>
        /// Formats value to string
        /// </summary>
        /// <param name="value">Original value</param>
        /// <returns>Formatted value</returns>
        string Format(T? value);
    }

    /// <summary>
    /// IStringFormatter
    /// </summary>
    public interface IStringFormatter : IResolvable
    {
        /// <summary>
        /// Formats value to string
        /// </summary>
        /// <param name="value">Original value</param>
        /// <returns>Formatted value</returns>
        string Format(object? value);
    }
}