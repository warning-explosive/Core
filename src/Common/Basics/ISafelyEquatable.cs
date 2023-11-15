namespace SpaceEngineers.Core.Basics
{
    /// <summary>
    /// ISafelyEquatable
    /// </summary>
    /// <typeparam name="T">T type-argument</typeparam>
    public interface ISafelyEquatable<T>
    {
        /// <summary>
        /// Non nullable analog of Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Comparison result</returns>
        bool SafeEquals(T other);
    }
}