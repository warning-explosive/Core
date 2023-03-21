namespace SpaceEngineers.Core.CrossCuttingConcerns.ObjectBuilder
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Basics;

    internal class DateOnlyTypeConverter : TypeConverter
    {
        private static readonly ConstructorInfo DateOnlyCctor = TypeExtensions
                .FindType("System.Private.CoreLib System.DateOnly")
                .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance)
                .Single(cctor => cctor.GetParameters().Length == 3
                                 && cctor.GetParameters().All(parameter => parameter.ParameterType == typeof(int)))
            ?? throw new InvalidOperationException("Unable to find DateOnly constructor");

        public override bool CanConvertFrom(
            ITypeDescriptorContext context,
            Type sourceType)
        {
            return sourceType == typeof(DateTime);
        }

        public sealed override object ConvertFrom(
            ITypeDescriptorContext context,
            CultureInfo culture,
            object value)
        {
            return value is DateTime dateTime
                ? DateOnlyCctor.Invoke(new object[] { dateTime.Year, dateTime.Month, dateTime.Day })
                : throw new InvalidOperationException($"{nameof(DateOnlyTypeConverter)} support only {typeof(DateTime)} values");
        }
    }
}