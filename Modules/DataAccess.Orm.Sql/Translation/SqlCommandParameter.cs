namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections;
    using System.Text;
    using Basics;

    /// <summary>
    /// SqlCommandParameter
    /// </summary>
    public class SqlCommandParameter
    {
        /// <summary> .cctor </summary>
        /// <param name="name">Name</param>
        /// <param name="value">Value</param>
        /// <param name="type">Type</param>
        /// <param name="isJsonValue">Should value be converted to json object</param>
        public SqlCommandParameter(string name, object? value, Type type, bool isJsonValue = false)
        {
            Name = name;
            Value = value;
            Type = type;
            IsJsonValue = isJsonValue;
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
        /// Should value be converted to json object
        /// </summary>
        public bool IsJsonValue { get; }

        /// <summary>
        /// Deconstruct
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="value">Value</param>
        /// <param name="type">Type</param>
        /// <param name="isJsonValue">Should value be converted to json object</param>
        public void Deconstruct(
            out string name,
            out object? value,
            out Type type,
            out bool isJsonValue)
        {
            name = Name;
            value = Value;
            type = Type;
            isJsonValue = IsJsonValue;
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
                    : Value.ToString();

            sb.Append(value);

            var type = Type == Type.ExtractGenericArgumentAtOrSelf(typeof(Nullable<>))
                ? Type.Name
                : Type.ExtractGenericArgumentAtOrSelf(typeof(Nullable<>)).Name + "?";

            if (IsJsonValue)
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