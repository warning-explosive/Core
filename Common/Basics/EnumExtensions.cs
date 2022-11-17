namespace SpaceEngineers.Core.Basics
{
    using System;

    /// <summary>
    /// Enum extensions
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Does source enum have the specified flag
        /// </summary>
        /// <param name="source">Source enum</param>
        /// <param name="flag">Expected flag</param>
        /// <typeparam name="TEnum">TEnum type-argument</typeparam>
        /// <returns>Result of check</returns>
        public static bool HasFlag<TEnum>(this TEnum? source, TEnum flag)
            where TEnum : Enum
        {
            return flag.HasFlag(flag);
        }
    }
}