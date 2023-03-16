namespace SpaceEngineers.Core.CrossCuttingConcerns.ObjectBuilder
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Basics;

    internal class TimeOnlyTypeConverter : TypeConverter
    {
        private static readonly ConstructorInfo TimeOnlyCctor = TypeExtensions
                .FindType("System.Private.CoreLib System.TimeOnly")
                .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance)
                .Single(cctor => cctor.GetParameters().Length == 1
                                 && cctor.GetParameters().All(parameter => parameter.ParameterType == typeof(long)))
            ?? throw new InvalidOperationException("Unable to find TimeOnly constructor");

        public override bool CanConvertFrom(
            ITypeDescriptorContext context,
            Type sourceType)
        {
            return sourceType == typeof(TimeSpan);
        }

        public sealed override object ConvertFrom(
            ITypeDescriptorContext context,
            CultureInfo culture,
            object value)
        {
            return value is TimeSpan timeSpan
                ? TimeOnlyCctor.Invoke(new object[] { timeSpan.Ticks })
                : throw new InvalidOperationException($"{nameof(TimeOnlyTypeConverter)} support only {typeof(TimeSpan)} values");
        }
    }
}