namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Internals
{
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
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

        public Task<IQuery> Translate(Expression expression, CancellationToken token)
        {
            var intermediateExpression = _translator.Translate(expression);

            return this
                .CallMethod(nameof(Translate))
                .WithTypeArgument(intermediateExpression.GetType())
                .WithArguments(intermediateExpression, token)
                .Invoke<Task<IQuery>>();
        }

        private Task<IQuery> Translate<TExpression>(TExpression expression, CancellationToken token)
            where TExpression : IIntermediateExpression
        {
            return _dependencyContainer
                .Resolve<IQueryTranslator<TExpression>>()
                .Translate(expression, token);
        }
    }
}