namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Postgres.Translation
{
    using System;
    using System.Linq;
    using System.Text;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Translation;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class SetExpressionTranslator : ISqlExpressionTranslator<SetExpression>,
                                             IResolvable<ISqlExpressionTranslator<SetExpression>>,
                                             ICollectionResolvable<ISqlExpressionTranslator>
    {
        private readonly ISqlExpressionTranslatorComposite _translator;

        public SetExpressionTranslator(ISqlExpressionTranslatorComposite translator)
        {
            _translator = translator;
        }

        public string Translate(ISqlExpression expression, int depth)
        {
            return expression is SetExpression setExpression
                ? Translate(setExpression, depth)
                : throw new NotSupportedException($"Unsupported sql expression type {expression.GetType()}");
        }

        public string Translate(SetExpression expression, int depth)
        {
            var sb = new StringBuilder();

            sb.AppendLine(_translator.Translate(expression.Source, depth));
            sb.Append("SET ");
            sb.Append(expression
                .Assignments
                .Select(assignment => _translator.Translate(assignment, depth))
                .ToString(", "));

            return sb.ToString();
        }
    }
}