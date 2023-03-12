namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.ObjectTransformers
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using Basics;

    internal class DbNullTypeConverter : TypeConverter
    {
        public override bool CanConvertTo(
            ITypeDescriptorContext context,
            Type destinationType)
        {
            return destinationType.IsReference() || destinationType.IsNullable();
        }

        public override object ConvertTo(
            ITypeDescriptorContext context,
            CultureInfo culture,
            object value,
            Type destinationType)
        {
            return value is DBNull
                ? null!
                : throw new InvalidOperationException($"{nameof(DbNullTypeConverter)} support only {typeof(DBNull)} values");
        }

        public override bool CanConvertFrom(
            ITypeDescriptorContext context,
            Type sourceType)
        {
            return sourceType == typeof(DBNull);
        }

        public sealed override object ConvertFrom(
            ITypeDescriptorContext context,
            CultureInfo culture,
            object value)
        {
            return value is DBNull
                ? null!
                : throw new InvalidOperationException($"{nameof(DbNullTypeConverter)} support only {typeof(DBNull)} values");
        }
    }
}