namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions.Container;
    using Expressions;
    using Orm.Linq;

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
                .Resolve<IIntermediateQueryTranslator<TExpression>>()
                .Translate(expression, token);
        }
    }
}