namespace SpaceEngineers.Core.CrossCuttingConcerns.StringFormatter
{
    using System;
    using Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    internal class DateTimeStringFormatter : IStringFormatter<DateTime>
    {
        public string Format(DateTime value)
        {
            return value.ToString("O");
        }
    }
}