namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// Enum extensions
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Get enum flags values flatten to array
        /// </summary>
        /// <param name="source">source</param>
        /// <returns>Flatten enum flags values</returns>
        public static Array EnumFlagsValues(this Enum source)
        {
            var type = source.GetType();

            return Enum
                .GetValues(type)
                .OfType<Enum>()
                .Where(flag =>
                {
                    // Checks whether x is a power of 2
                    var value = Convert.ToInt64(flag, CultureInfo.InvariantCulture);
                    return value != 0 && (value & (value - 1)) == 0;
                })
                .Where(source.HasFlag)
                .Select(flag => Convert.ChangeType(flag, type, CultureInfo.InvariantCulture))
                .ToArray();
        }
    }
}