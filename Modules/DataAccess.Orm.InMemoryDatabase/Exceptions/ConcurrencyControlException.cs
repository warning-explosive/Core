namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Exceptions
{
    using System;
    using System.Globalization;

    /// <summary>
    /// ConcurrencyControlException
    /// </summary>
    public sealed class ConcurrencyControlException : Exception
    {
        private const string Format = "Multiversional concurrency control violation. {0} have been updated and has {1} version. Unable to apply version {2}";

        /// <summary> .cctor </summary>
        /// <param name="violationType">Violation type</param>
        /// <param name="actualVersion">Actual version</param>
        /// <param name="expectedVersion">Expected version</param>
        public ConcurrencyControlException(string violationType, DateTime actualVersion, DateTime expectedVersion)
            : base(string.Format(CultureInfo.InvariantCulture, Format, violationType, actualVersion, expectedVersion))
        {
        }
    }
}