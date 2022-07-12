namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Host.Model
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// EnPostgreSqlDataType
    /// </summary>
    public enum EnPostgreSqlDataType
    {
        /// <summary>
        /// UUID type
        /// </summary>
        Uuid,

        /// <summary>
        /// boolean
        /// 1 byte
        /// state of true or false
        /// </summary>
        Boolean,

        /// <summary>
        /// character type
        /// variable-length with limit
        /// </summary>
        Varchar,

        /// <summary>
        /// character type
        /// variable unlimited length
        /// </summary>
        Text,

        /// <summary>
        /// smallint
        /// 2 bytes
        /// -32768 to +32767
        /// </summary>
        SmallInt,

        /// <summary>
        /// integer
        /// 4 bytes
        /// -2147483648 to +2147483647
        /// </summary>
        [SuppressMessage("Analysis", "CA1720", Justification = "desired name")]
        Integer,

        /// <summary>
        /// bigint
        /// 8 bytes
        /// -9223372036854775808 to +9223372036854775807
        /// </summary>
        BigInt,

        /// <summary>
        /// numeric
        /// variable, user-specified precision
        /// up to 131072 digits before the decimal point; up to 16383 digits after the decimal point
        /// </summary>
        Numeric,

        /// <summary>
        /// 4713 BC to 294276 AD
        /// 1 microsecond resolution
        /// without time zone
        /// </summary>
        Timestamp,

        /// <summary>
        /// time interval
        /// 16 bytes
        /// -178000000 years to 178000000 years
        /// 1 microsecond resolution
        /// </summary>
        Interval,

        /// <summary>
        /// date (no time of day)
        /// 4 bytes
        /// 4713 BC to 5874897 AD
        /// 1 day resolution
        /// </summary>
        Date,

        /// <summary>
        /// time of day (no date)
        /// 8 bytes
        /// 00:00:00 to 24:00:00
        /// 1 microsecond resolution
        /// </summary>
        Time,

        /// <summary>
        /// transaction id (xid)
        /// </summary>
        Xid
    }
}