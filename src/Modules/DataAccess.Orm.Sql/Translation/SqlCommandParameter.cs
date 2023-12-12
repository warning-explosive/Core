namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections;
    using System.Text;
    using Basics;
    using Linq;

    /// <summary>
    /// SqlCommandParameter
    /// </summary>
    public class SqlCommandParameter
    {
        /// <summary> .cctor </summary>
        /// <param name="name">Name</param>
        /// <param name="value">Value</param>
        /// <param name="type">Type</param>
        public SqlCommandParameter(string name, object? value, Type type)
        {
            Name = name;
            Value = value;
            Type = type;
        }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Value
        /// </summary>
        public object? Value { get; }

        /// <summary>
        /// Type
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Deconstruct
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="value">Value</param>
        /// <param name="type">Type</param>
        public void Deconstruct(
            out string name,
            out object? value,
            out Type type)
        {
            name = Name;
            value = Value;
            type = Type;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(Name);

            sb.Append("=");

            var value = Value == null
                ? "NULL"
                : Value.GetType().IsCollection()
                    ? $"[{((IEnumerable)Value).AsEnumerable<object>().ToString(", ")}]"
                    : Value is string str && string.Equals(str, string.Empty, StringComparison.OrdinalIgnoreCase)
                        ? "empty_string"
                        : Value.ToString();

            sb.Append(value);

            var type = Type == Type.ExtractGenericArgumentAtOrSelf(typeof(Nullable<>))
                ? Type.Name
                : Type.ExtractGenericArgumentAtOrSelf(typeof(Nullable<>)).Name + "?";

            if (typeof(DatabaseJsonObject).IsAssignableFrom(Type))
            {
                sb.Append($"({type}, JSON)");
            }
            else
            {
                sb.Append($"({type})");
            }

            return sb.ToString();
        }
    }
}