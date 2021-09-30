namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    internal class FallbackQueryParameterTranslator<T> : IQueryParameterTranslator<T?>
    {
        public string Translate(T? value)
        {
            return value?.ToString() ?? "NULL";
        }
    }
}