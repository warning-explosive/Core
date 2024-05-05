namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class SqlExpressionTranslatorComposite : ISqlExpressionTranslatorComposite,
                                                      IResolvable<ISqlExpressionTranslatorComposite>
    {
        private readonly IEnumerable<ISqlExpressionTranslator> _sqlExpressionTranslators;

        private IReadOnlyDictionary<Type, ISqlExpressionTranslator>? _map;

        public SqlExpressionTranslatorComposite(IEnumerable<ISqlExpressionTranslator> sqlExpressionTranslators)
        {
            _sqlExpressionTranslators = sqlExpressionTranslators;
        }

        public string Translate(ISqlExpression expression, int depth)
        {
            _map ??= _sqlExpressionTranslators.ToDictionary(static translator => translator.GetType().ExtractGenericArgumentAt(typeof(ISqlExpressionTranslator<>)));

            return _map.TryGetValue(expression.GetType(), out var translator)
                ? translator.Translate(expression, depth)
                : throw new NotSupportedException($"Unsupported sql expression type {expression.GetType()}");
        }
    }
}