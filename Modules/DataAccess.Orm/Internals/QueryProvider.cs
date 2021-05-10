namespace SpaceEngineers.Core.DataAccess.Orm.Internals
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using Abstractions;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;
    using Contract.Abstractions;
    using CrossCuttingConcerns.Api.Abstractions;
    using Dapper;
    using Settings;
    using SettingsManager.Abstractions;

    [Component(EnLifestyle.Scoped)]
    internal class QueryProvider<T> : IAsyncQueryProvider<T>,
                                      IQueryProvider,
                                      IExternalResolvable<IQueryProvider>
    {
        private readonly ISettingsManager<OrmSettings> _ormSettingsProvider;
        private readonly IQueryTranslator _translator;
        private readonly IDatabaseTransaction _transaction;
        private readonly IObjectBuilder<T> _objectBuilder;

        public QueryProvider(
            ISettingsManager<OrmSettings> ormSettingsProvider,
            IQueryTranslator translator,
            IDatabaseTransaction transaction,
            IObjectBuilder<T> objectBuilder)
        {
            _ormSettingsProvider = ormSettingsProvider;
            _translator = translator;
            _transaction = transaction;
            _objectBuilder = objectBuilder;
        }

        public IQueryProvider AsQueryProvider()
        {
            return this;
        }

        public IQueryable<T> CreateQuery(Expression expression)
        {
            return new Queryable<T>(this, expression);
        }

        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            return CreateQuery(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return (IQueryable<TElement>)CreateQuery(expression);
        }

        public object Execute(Expression expression)
        {
            return GetType()
                .CallMethod(nameof(Execute))
                .WithTypeArgument(expression.Type)
                .WithArgument(expression)
                .ForInstance(this)
                .Invoke<object>();
        }

        public TResult Execute<TResult>(Expression expression)
        {
            var enumerableResult = GetType()
                .CallMethod(nameof(ExecuteAsync))
                .WithArgument(expression)
                .WithArgument(CancellationToken.None)
                .ForInstance(this)
                .Invoke<IAsyncEnumerable<object>>()
                .AsEnumerable()
                .Result;

            var itemType = typeof(TResult).UnwrapTypeParameter(typeof(IEnumerable<>));

            return itemType == typeof(TResult)
                ? (TResult)enumerableResult.Single()
                : (TResult)enumerableResult;
        }

        public async IAsyncEnumerable<T> ExecuteAsync(
            Expression expression,
            [EnumeratorCancellation] CancellationToken token)
        {
            var ormSettings = await _ormSettingsProvider
                .Get()
                .ConfigureAwait(false);

            var query = _translator.Translate(expression);

            var transaction = await _transaction
                .Open(token)
                .ConfigureAwait(false);

            var dynamicResult = await transaction
                .Connection
                .QueryAsync(query.Query, query.Parameters, transaction, ormSettings.QueryTimeout.Seconds, CommandType.Text)
                .ConfigureAwait(false);

            foreach (var values in dynamicResult)
            {
                yield return _objectBuilder.Build(values as IDictionary<string, object>);
            }
        }
    }
}