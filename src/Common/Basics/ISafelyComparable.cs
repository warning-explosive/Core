namespace SpaceEngineers.Core.Basics
{
    /// <summary>
    /// ISafelyComparable
    /// </summary>
    /// <typeparam name="T">T type-argument</typeparam>
    public interface ISafelyComparable<in T>
    {
        /// <summary>
        /// Non nullable analog of CompareTo
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Comparison result</returns>
        int SafeCompareTo(T other);
    }
}