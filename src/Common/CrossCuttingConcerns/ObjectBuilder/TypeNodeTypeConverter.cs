namespace SpaceEngineers.Core.CrossCuttingConcerns.ObjectBuilder
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using Basics;

    internal class TypeNodeTypeConverter : TypeConverter
    {
        public override bool CanConvertTo(
            ITypeDescriptorContext context,
            Type destinationType)
        {
            return context is ObjectBuilderTypeDescriptorContext
                   && destinationType == typeof(string);
        }

        public override object ConvertTo(
            ITypeDescriptorContext context,
            CultureInfo culture,
            object value,
            Type destinationType)
        {
            return value is Type type
                ? TypeNode.FromType(type).ToString()
                : throw new InvalidOperationException($"{nameof(TypeNodeTypeConverter)} support only {typeof(Type)} values");
        }

        public override bool CanConvertFrom(
            ITypeDescriptorContext context,
            Type sourceType)
        {
            return context is ObjectBuilderTypeDescriptorContext
                   && sourceType == typeof(string);
        }

        public sealed override object ConvertFrom(
            ITypeDescriptorContext context,
            CultureInfo culture,
            object value)
        {
            return value is string str
                ? TypeNode.ToType(TypeNode.FromString(str))
                : throw new InvalidOperationException($"{nameof(TypeNodeTypeConverter)} support only {typeof(string)} values");
        }
    }
}