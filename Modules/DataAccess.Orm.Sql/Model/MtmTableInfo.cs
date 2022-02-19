namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Basics;

    /// <summary>
    /// MtmTableInfo
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class MtmTableInfo : TableInfo,
                                IEquatable<MtmTableInfo>,
                                ISafelyEquatable<MtmTableInfo>
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="left">Left (owner)</param>
        /// <param name="right">Right (relation)</param>
        /// <param name="modelProvider">IModelProvider</param>
        public MtmTableInfo(
            Type type,
            Type left,
            Type right,
            IModelProvider modelProvider)
            : base(type, modelProvider)
        {
            Left = left;
            Right = right;
        }

        /// <summary>
        /// Left (owner)
        /// </summary>
        public Type Left { get; }

        /// <summary>
        /// Right (relation)
        /// </summary>
        public Type Right { get; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left MtmTableInfo</param>
        /// <param name="right">Right MtmTableInfo</param>
        /// <returns>equals</returns>
        public static bool operator ==(MtmTableInfo? left, MtmTableInfo? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left MtmTableInfo</param>
        /// <param name="right">Right MtmTableInfo</param>
        /// <returns>not equals</returns>
        public static bool operator !=(MtmTableInfo? left, MtmTableInfo? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        [SuppressMessage("Analysis", "CA1308", Justification = "sql script readability")]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(MtmTableInfo? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(MtmTableInfo other)
        {
            return base.SafeEquals(other);
        }

        #endregion
    }
}