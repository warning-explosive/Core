namespace SpaceEngineers.Core.CrossCuttingConcerns.StringFormatter
{
    /// <summary>
    /// IStringFormatter
    /// </summary>
    /// <typeparam name="T">T type-argument</typeparam>
    public interface IStringFormatter<T>
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
    public interface IStringFormatter
    {
        /// <summary>
        /// Formats value to string
        /// </summary>
        /// <param name="value">Original value</param>
        /// <returns>Formatted value</returns>
        string Format(object? value);
    }
}