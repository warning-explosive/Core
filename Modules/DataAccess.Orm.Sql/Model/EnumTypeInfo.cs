namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using Basics;

    internal class EnumTypeInfo : IModelInfo,
                                IEquatable<EnumTypeInfo>,
                                ISafelyEquatable<EnumTypeInfo>
    {
        public EnumTypeInfo(string schema, Type type)
        {
            Schema = schema;
            Type = type;
        }

        public string Schema { get; }

        public Type Type { get; }

        public string Name => $@"""{Schema}"".""{Type.Name}""";

        #region IEquatable

        public static bool operator ==(EnumTypeInfo? left, EnumTypeInfo? right)
        {
            return Equatable.Equals(left, right);
        }

        public static bool operator !=(EnumTypeInfo? left, EnumTypeInfo? right)
        {
            return !Equatable.Equals(left, right);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                Schema.GetHashCode(StringComparison.OrdinalIgnoreCase),
                Type);
        }

        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        public bool Equals(EnumTypeInfo? other)
        {
            return Equatable.Equals(this, other);
        }

        public bool SafeEquals(EnumTypeInfo other)
        {
            return Schema.Equals(other.Schema, StringComparison.OrdinalIgnoreCase)
                   && Type == other.Type;
        }

        #endregion
    }
}