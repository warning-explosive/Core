namespace SpaceEngineers.Core.Basics
{
    using System;

    /// <summary>
    /// Integer extensions
    /// </summary>
    public static class IntegerExtensions
    {
        /// <summary>
        /// Between include bounds
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="start">Range start</param>
        /// <param name="end">Range end</param>
        /// <returns>Index included in range</returns>
        public static bool BetweenInclude(this int index, int start, int end)
        {
            return start <= index && index <= end;
        }

        /// <summary>
        /// Between include bounds
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="range">Range</param>
        /// <returns>Index included in range</returns>
        public static bool BetweenInclude(this int index, Range range)
        {
            return BetweenInclude(index, range.Start.Value, range.End.Value);
        }

        /// <summary>
        /// Between include bounds
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="start">Range start</param>
        /// <param name="end">Range end</param>
        /// <returns>Index included in range</returns>
        public static bool BetweenInclude(this uint index, uint start, uint end)
        {
            return start <= index && index <= end;
        }

        /// <summary>
        /// Between include bounds
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="range">Range</param>
        /// <returns>Index included in range</returns>
        public static bool BetweenInclude(this uint index, Range range)
        {
            CheckBoundaries(range.Start.Value, range.End.Value);

            return BetweenInclude(index, (uint)range.Start.Value, (uint)range.End.Value);
        }

        private static void CheckBoundaries(int start, int end)
        {
            if (start < 0)
            {
                throw new ArgumentException($"{start} boundary must be greater than zero", nameof(start));
            }

            if (end < 0)
            {
                throw new ArgumentException($"{end} boundary must be greater than zero", nameof(end));
            }
        }
    }
}