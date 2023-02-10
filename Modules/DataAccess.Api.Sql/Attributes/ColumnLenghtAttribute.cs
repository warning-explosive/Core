namespace SpaceEngineers.Core.DataAccess.Api.Sql.Attributes
{
    using System;

    /// <summary>
    /// ColumnLenghtAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ColumnLenghtAttribute : Attribute
    {
        /// <summary> .cctor </summary>
        /// <param name="length">Length</param>
        public ColumnLenghtAttribute(uint length)
        {
            Length = length;
        }

        /// <summary>
        /// length
        /// </summary>
        public uint Length { get; }
    }
}