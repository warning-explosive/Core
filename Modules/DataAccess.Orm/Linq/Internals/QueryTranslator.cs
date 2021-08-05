namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Internals
{
    using System.Linq.Expressions;
    using Abstractions;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;

    [Component(EnLifestyle.Scoped)]
    internal class QueryTranslator : IQueryTranslator
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

            return this
                .CallMethod(nameof(Translate))
                .WithTypeArgument(intermediateExpression.GetType())
                .WithArgument(intermediateExpression)
                .Invoke<IQuery>();
        }

        private IQuery Translate<TExpression>(TExpression expression)
            where TExpression : IIntermediateExpression
        {
            return _dependencyContainer
                .Resolve<IQueryTranslator<TExpression>>()
                .Translate(expression);
        }
    }
}