namespace SpaceEngineers.Core.Basics;

using System;
using System.Globalization;
using System.Linq;

public static class EnumExtensions
{
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