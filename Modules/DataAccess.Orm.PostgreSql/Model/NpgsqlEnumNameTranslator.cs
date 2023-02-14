namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Model
{
    using Npgsql;

    internal class NpgsqlEnumNameTranslator : INpgsqlNameTranslator
    {
        public string TranslateTypeName(string clrName)
        {
            return clrName;
        }

        public string TranslateMemberName(string clrName)
        {
            return clrName;
        }
    }
}