namespace SpaceEngineers.Core.CrossCuttingConcerns.StringFormatter
{
    using System;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    internal class DateTimeStringFormatter : IStringFormatter<DateTime>,
                                             IResolvable<IStringFormatter<DateTime>>
    {
        public string Format(DateTime value)
        {
            return value.ToString("O");
        }
    }
}