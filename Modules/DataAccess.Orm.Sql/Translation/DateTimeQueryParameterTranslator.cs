namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    internal class DateTimeQueryParameterTranslator : IQueryParameterTranslator<DateTime>
    {
        public string Translate(DateTime value)
        {
            return $@"'{value.ToUniversalTime():O}'";
        }
    }
}