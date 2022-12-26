namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Linq.Expressions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot;
    using Expressions;
    using Linq;

    [Component(EnLifestyle.Singleton)]
    internal class QueryTranslator : IQueryTranslator,
                                     IResolvable<IQueryTranslator>
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IExpressionTranslator _translator;

        public QueryTranslator(
            IDependencyContainer dependencyContainer,
            IExpressionTranslator translator)
        {
            _dependencyContainer = dependencyContainer;
            _translator = translator;
        }

        public IQuery Translate(Expression expression)
        {
            var intermediateExpression = _translator.Translate(expression);

            return _dependencyContainer
                .ResolveGeneric(typeof(IIntermediateQueryTranslator<>), intermediateExpression.GetType())
                .CallMethod(nameof(IIntermediateQueryTranslator<IIntermediateExpression>.Translate))
                .WithArguments(intermediateExpression)
                .Invoke<IQuery>();
        }
    }
}