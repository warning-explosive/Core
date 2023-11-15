namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Linq
{
    using System;

    /// <summary>
    /// SqlExpressionsExtensions
    /// </summary>
    public static class SqlExpressionsExtensions
    {
        private const char Percent = '%';

        /// <summary>
        /// Like
        /// </summary>
        /// <param name="source">Source string</param>
        /// <param name="pattern">Pattern</param>
        /// <returns>Does the source contain entry with specified pattern or not</returns>
        public static bool Like(this string source, string pattern)
        {
            var starts = pattern.StartsWith(Percent);
            var ends = pattern.EndsWith(Percent);
            var cleanPattern = pattern.Trim(Percent);

            if (starts && ends)
            {
                return source.Contains(cleanPattern, StringComparison.Ordinal);
            }
            else if (!starts && ends)
            {
                return source.StartsWith(cleanPattern, StringComparison.Ordinal);
            }
            else if (starts && !ends)
            {
                return source.EndsWith(cleanPattern, StringComparison.Ordinal);
            }
            else
            {
                return source.Equals(pattern, StringComparison.Ordinal);
            }
        }

        /// <summary>
        /// IsNull
        /// </summary>
        /// <param name="source">Source string</param>
        /// <returns>Does the value represent null</returns>
        public static bool IsNull(this object? source)
        {
            return source is null;
        }

        /// <summary>
        /// IsNull
        /// </summary>
        /// <param name="source">Source string</param>
        /// <returns>Does the value represent null</returns>
        public static bool IsNotNull(this object? source)
        {
            return source is not null;
        }

        /// <summary>
        /// Represents an assign binary operator
        /// </summary>
        /// <param name="left">left operand</param>
        /// <param name="right">right operand</param>
        /// <typeparam name="T">T type-argument</typeparam>
        public static void Assign<T>(this T left, T right)
        {
            throw new InvalidOperationException($"Method {nameof(Assign)} shouldn't be used outside of expression trees");
        }

        /// <summary>
        /// Does json object have an attribute with specified name
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="name">Json attribute name</param>
        /// <returns>Json object attribute existence</returns>
        public static bool HasJsonAttribute(this DatabaseJsonObject source, string name)
        {
            throw new InvalidOperationException($"Method {nameof(HasJsonAttribute)} shouldn't be used outside of expression trees");
        }

        /// <summary>
        /// Gets value of json object attribute by specified name
        /// </summary>
        /// <param name="jsonObject">Json object</param>
        /// <param name="name">Json attribute name</param>
        /// <typeparam name="TValue">TValue type-argument</typeparam>
        /// <returns>Json object attribute value</returns>
        public static DatabaseJsonObject<TValue> GetJsonAttribute<TValue>(this DatabaseJsonObject jsonObject, string name)
        {
            throw new InvalidOperationException($"Method {nameof(GetJsonAttribute)} shouldn't be used outside of expression trees");
        }

        /// <summary>
        /// Concatenates two json objects into single one
        /// </summary>
        /// <param name="left">Left json object</param>
        /// <param name="right">Right json object</param>
        /// <typeparam name="TValue">TValue type-argument</typeparam>
        /// <returns>Concatenated json object</returns>
        public static DatabaseJsonObject<TValue> ConcatJsonObjects<TValue>(this DatabaseJsonObject left, DatabaseJsonObject right)
        {
            throw new InvalidOperationException($"Method {nameof(ConcatJsonObjects)} shouldn't be used outside of expression trees");
        }

        /// <summary>
        /// Excludes an attribute from json object
        /// </summary>
        /// <param name="jsonObject">Json object</param>
        /// <param name="name">Json attribute name</param>
        /// <returns>Altered json object</returns>
        public static DatabaseJsonObject ExcludeJsonAttribute(this DatabaseJsonObject jsonObject, string name)
        {
            throw new InvalidOperationException($"Method {nameof(ExcludeJsonAttribute)} shouldn't be used outside of expression trees");
        }

        /// <summary>
        /// Converts object to json object wrapper necessary for translation
        /// </summary>
        /// <param name="value">Value</param>
        /// <typeparam name="TValue">TValue type-argument</typeparam>
        /// <returns>JsonObject translation wrapper</returns>
        public static DatabaseJsonObject<TValue> AsJsonObject<TValue>(this TValue value)
        {
            return new DatabaseJsonObject<TValue>(value);
        }
    }
}