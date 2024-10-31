namespace SpaceEngineers.Core.Basics;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

public static class StringExtensions
{
    public static string StartFromCapitalLetter(this string source)
    {
        if (source.IsNullOrEmpty())
        {
            return source;
        }

        return string.Create(
            source.Length,
            source,
            static (buffer, source) =>
            {
                buffer[0] = char.ToUpper(source[0], CultureInfo.InvariantCulture);
                source.AsSpan(1).ToLowerInvariant(buffer.Slice(1));
            });
    }

    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? source)
    {
        return string.IsNullOrEmpty(source);
    }

    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? source)
    {
        return string.IsNullOrWhiteSpace(source);
    }

    public static string Format(this string format, params object[] args)
    {
        return string.Format(CultureInfo.InvariantCulture, format, args);
    }

    public static string Format(this string format, IFormatProvider formatProvider, params object[] args)
    {
        return string.Format(formatProvider, format, args);
    }
}