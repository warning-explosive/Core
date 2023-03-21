namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// UniqueIdentifiedEqualityComparer
    /// </summary>
    public class UniqueIdentifiedEqualityComparer : IEqualityComparer<IUniqueIdentified>
    {
        /// <inheritdoc />
        public bool Equals(IUniqueIdentified? x, IUniqueIdentified? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null)
                || ReferenceEquals(y, null))
            {
                return false;
            }

            return x.GetType() == y.GetType()
                   && x.PrimaryKey.Equals(y.PrimaryKey);
        }

        /// <inheritdoc />
        public int GetHashCode(IUniqueIdentified obj)
        {
            return HashCode.Combine(obj.GetType(), obj.PrimaryKey);
        }
    }
}