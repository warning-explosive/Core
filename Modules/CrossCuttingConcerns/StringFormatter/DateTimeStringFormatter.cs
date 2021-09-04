namespace SpaceEngineers.Core.CrossCuttingConcerns.StringFormatter
{
    using System;
    using Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    internal class DateTimeStringFormatter : IStringFormatter<DateTime>
    {
        public string Format(DateTime value)
        {
            return value.ToString("O");
        }
    }
}