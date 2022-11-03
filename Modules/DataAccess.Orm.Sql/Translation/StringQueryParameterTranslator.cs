namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    internal class StringQueryParameterTranslator : IQueryParameterTranslator<string>,
                                                    IResolvable<IQueryParameterTranslator<string>>
    {
        private static readonly IReadOnlyDictionary<string, string> Replacements = new Dictionary<string, string>
        {
            ["\'"] = "\'\'"
        };

        public string Translate(string value)
        {
            return $"'{EscapeSpecialCharacters(value)}'";
        }

        private static string EscapeSpecialCharacters(string value)
        {
            return Replacements.Aggregate(value, (acc, pair) => acc.Replace(pair.Key, pair.Value, StringComparison.OrdinalIgnoreCase));
        }
    }
}