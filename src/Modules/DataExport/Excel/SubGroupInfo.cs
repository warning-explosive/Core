namespace SpaceEngineers.Core.DataExport.Excel
{
    using System;
    using Basics;

    /// <summary>
    /// SubGroupInfo
    /// </summary>
    /// <typeparam name="TRow">TRow type-argument</typeparam>
    public class SubGroupInfo<TRow> : IEquatable<SubGroupInfo<TRow>>,
                                      ISafelyEquatable<SubGroupInfo<TRow>>
    {
        /// <summary> .cctor </summary>
        /// <param name="name">Name</param>
        /// <param name="keySelector">keySelector</param>
        /// <param name="position">position</param>
        public SubGroupInfo(
            string name,
            Func<TRow, string> keySelector,
            SubGroupPosition position = SubGroupPosition.Top)
        {
            Name = name;
            KeySelector = keySelector;
            Position = position;
        }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Key selector
        /// </summary>
        public Func<TRow, string> KeySelector { get; }

        /// <summary>
        /// Position
        /// </summary>
        public SubGroupPosition Position { get; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left SubGroupInfo</param>
        /// <param name="right">Right SubGroupInfo</param>
        /// <returns>equals</returns>
        public static bool operator ==(SubGroupInfo<TRow>? left, SubGroupInfo<TRow>? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left SubGroupInfo</param>
        /// <param name="right">Right SubGroupInfo</param>
        /// <returns>not equals</returns>
        public static bool operator !=(SubGroupInfo<TRow>? left, SubGroupInfo<TRow>? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public bool SafeEquals(SubGroupInfo<TRow> other)
        {
            return string.Equals(Name, other.Name, StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public bool Equals(SubGroupInfo<TRow>? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Name.GetHashCode(StringComparison.Ordinal);
        }

        #endregion
    }
}