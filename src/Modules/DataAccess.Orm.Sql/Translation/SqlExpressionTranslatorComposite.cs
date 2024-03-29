namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot;
    using Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class SqlExpressionTranslatorComposite : ISqlExpressionTranslatorComposite,
                                                      IResolvable<ISqlExpressionTranslatorComposite>
    {
        private readonly IDependencyContainer _dependencyContainer;

        private IReadOnlyDictionary<Type, ISqlExpressionTranslator>? _map;

        public SqlExpressionTranslatorComposite(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public string Translate(ISqlExpression expression, int depth)
        {
            _map ??= _dependencyContainer
                .ResolveCollection<ISqlExpressionTranslator>()
                .ToDictionary(static translator => translator.GetType().ExtractGenericArgumentAt(typeof(ISqlExpressionTranslator<>)));

            return _map.TryGetValue(expression.GetType(), out var translator)
                ? translator.Translate(expression, depth)
                : throw new NotSupportedException($"Unsupported sql expression type {expression.GetType()}");
        }
    }
}