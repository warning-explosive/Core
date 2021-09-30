namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    internal class StringQueryParameterTranslator : IQueryParameterTranslator<string?>
    {
        public string Translate(string? value)
        {
            return value == null
                ? "NULL"
                : $"'{value}'";
        }
    }
}