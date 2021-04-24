namespace SpaceEngineers.Core.CrossCuttingConcerns
{
    using System;
    using Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Component(EnLifestyle.Singleton, EnComponentRegistrationKind.ManuallyRegistered)]
    internal class DateTimeStringFormatter : IStringFormatter<DateTime>
    {
        public string Format(DateTime value)
        {
            return value.ToString("O");
        }
    }
}