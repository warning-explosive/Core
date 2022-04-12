namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    internal class GuidQueryParameterTranslator : IQueryParameterTranslator<Guid>,
                                                  IResolvable<IQueryParameterTranslator<Guid>>
    {
        public string Translate(Guid value)
        {
            return $"'{value}'";
        }
    }
}