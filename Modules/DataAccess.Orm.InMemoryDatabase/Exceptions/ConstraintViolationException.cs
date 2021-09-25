namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Exceptions
{
    using System;
    using System.Globalization;

    /// <summary>
    /// ConstraintViolationException
    /// </summary>
    public sealed class ConstraintViolationException : Exception
    {
        private const string Format = "Primary key violation exception: {0} {1}";

        /// <summary> .cctor </summary>
        /// <param name="entityType">Entity type</param>
        /// <param name="primaryKey">Primary key</param>
        public ConstraintViolationException(Type entityType, object primaryKey)
            : base(string.Format(CultureInfo.InvariantCulture, Format, entityType.Name, primaryKey.ToString()))
        {
        }
    }
}