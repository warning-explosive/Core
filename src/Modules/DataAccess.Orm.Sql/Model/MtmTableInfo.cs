namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;
    using Basics;

    internal class MtmTableInfo : TableInfo,
                                  IEquatable<MtmTableInfo>,
                                  ISafelyEquatable<MtmTableInfo>
    {
        public MtmTableInfo(
            Type type,
            Type left,
            Type right,
            IReadOnlyDictionary<string, ColumnInfo> columns,
            IModelProvider modelProvider)
            : base(type, columns, modelProvider)
        {
            Left = left;
            Right = right;
        }

        public override bool IsMtmTable => true;

        public Type Left { get; }

        public Type Right { get; }

        #region IEquatable

        public static bool operator ==(MtmTableInfo? left, MtmTableInfo? right)
        {
            return Equatable.Equals(left, right);
        }

        public static bool operator !=(MtmTableInfo? left, MtmTableInfo? right)
        {
            return !Equatable.Equals(left, right);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        public bool Equals(MtmTableInfo? other)
        {
            return Equatable.Equals(this, other);
        }

        public bool SafeEquals(MtmTableInfo other)
        {
            return base.SafeEquals(other);
        }

        #endregion
    }
}