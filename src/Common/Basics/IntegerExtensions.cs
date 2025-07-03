namespace SpaceEngineers.Core.Basics;

using System;
using System.Collections.Generic;
using System.Linq;

public static class IntegerExtensions
{
    public static string AlphabetIndex(this int index)
    {
        var ranks = GetRanks(index, 'z' - 'a' + 1)
            .Reverse()
            .Select(rank => (char)('a' + rank))
            .ToArray();

        return new string(ranks);

        static IEnumerable<int> GetRanks(int index, int length)
        {
            var current = index;

            while (current >= length)
            {
                yield return current % length;
                current = (current / length) - 1;
            }

            yield return current;
        }
    }

    public static uint Log(this int result, uint basis)
    {
        return (uint)Math.Log(result, basis);
    }

    public static int Pow(this int basis, uint exp)
    {
        var result = 1;

        checked
        {
            while (exp != 0)
            {
                if ((exp & 1) == 1)
                {
                    result *= basis;
                }

                basis *= basis;
                exp >>= 1;
            }
        }

        return result;
    }

    public static bool BetweenInclude(this int index, int start, int end)
    {
        return BetweenInclude((uint)index, (uint)start, (uint)end);
    }

    public static bool BetweenInclude(this int index, Range range)
    {
        return BetweenInclude((uint)index, range);
    }

    public static bool BetweenInclude(this uint index, Range range)
    {
        CheckBoundaries(range.Start.Value, range.End.Value);

        return BetweenInclude(index, (uint)range.Start.Value, (uint)range.End.Value);

        static void CheckBoundaries(int start, int end)
        {
            if (start < 0)
            {
                throw new ArgumentException($"{start} boundary should be greater than zero", nameof(start));
            }

            if (end < 0)
            {
                throw new ArgumentException($"{end} boundary should be greater than zero", nameof(end));
            }
        }
    }

    public static bool BetweenInclude(this uint index, uint start, uint end)
    {
        return start <= index && index <= end;
    }
}