namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using Basics;

    /// <summary>
    /// EnumTypeInfo
    /// </summary>
    public class EnumTypeInfo : IModelInfo,
                                IEquatable<EnumTypeInfo>,
                                ISafelyEquatable<EnumTypeInfo>
    {
        /// <summary> .cctor </summary>
        /// <param name="schema">Schema</param>
        /// <param name="type">Type</param>
        public EnumTypeInfo(string schema, Type type)
        {
            Schema = schema;
            Type = type;
        }

        /// <summary>
        /// Schema
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// Type
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name => $@"""{Schema}"".""{Type.Name}""";

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left EnumTypeInfo</param>
        /// <param name="right">Right EnumTypeInfo</param>
        /// <returns>equals</returns>
        public static bool operator ==(EnumTypeInfo? left, EnumTypeInfo? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left EnumTypeInfo</param>
        /// <param name="right">Right EnumTypeInfo</param>
        /// <returns>not equals</returns>
        public static bool operator !=(EnumTypeInfo? left, EnumTypeInfo? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(
                Schema.GetHashCode(StringComparison.OrdinalIgnoreCase),
                Type);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(EnumTypeInfo? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(EnumTypeInfo other)
        {
            return Schema.Equals(other.Schema, StringComparison.OrdinalIgnoreCase)
                   && Type == other.Type;
        }

        #endregion
    }
}