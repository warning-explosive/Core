namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System;
    using System.Text;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Sql.Translation;
    using Sql.Translation.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class JsonAttributeExpressionTranslator : ISqlExpressionTranslator<JsonAttributeExpression>,
                                                       IResolvable<ISqlExpressionTranslator<JsonAttributeExpression>>,
                                                       ICollectionResolvable<ISqlExpressionTranslator>
    {
        private const string ExcludeJsonMetadataKeys = "CASE WHEN SUBSTRING(LTRIM({0}::text), 1, 1) = '{{' THEN {0} - array(SELECT * from jsonb_object_keys({0}) WHERE jsonb_object_keys like '$%') ELSE {0} END";

        private readonly ISqlExpressionTranslatorComposite _translator;

        public JsonAttributeExpressionTranslator(ISqlExpressionTranslatorComposite translator)
        {
            _translator = translator;
        }

        public string Translate(ISqlExpression expression, int depth)
        {
            return expression is JsonAttributeExpression jsonAttributeExpression
                ? Translate(jsonAttributeExpression, depth)
                : throw new NotSupportedException($"Unsupported sql expression type {expression.GetType()}");
        }

        public string Translate(JsonAttributeExpression expression, int depth)
        {
            var sb = new StringBuilder();

            sb.Append(_translator.Translate(expression.Source, depth));
            sb.Append('[');
            sb.Append(_translator.Translate(expression.Accessor, depth));
            sb.Append(']');

            return ExcludeJsonMetadataKeys.Format(sb.ToString());
        }
    }
}