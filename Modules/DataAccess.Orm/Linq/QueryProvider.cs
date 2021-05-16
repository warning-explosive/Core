namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System;
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
    using Dapper;
    using Settings;
    using SettingsManager.Abstractions;

    [Component(EnLifestyle.Scoped)]
    internal class QueryProvider : IAsyncQueryProvider,
                                   IExternalResolvable<IQueryProvider>
    {
        private readonly ISettingsManager<OrmSettings> _ormSettingsProvider;
        private readonly IQueryTranslator _translator;
        private readonly IDatabaseTransaction _transaction;

        public QueryProvider(
            ISettingsManager<OrmSettings> ormSettingsProvider,
            IQueryTranslator translator,
            IDatabaseTransaction transaction)
        {
            _ormSettingsProvider = ormSettingsProvider;
            _translator = translator;
            _transaction = transaction;
        }

        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            var itemType = expression.Type.UnwrapTypeParameter(typeof(IQueryable<>));

            return (IQueryable)Activator.CreateInstance(
                typeof(Queryable<>).MakeGenericType(itemType),
                this,
                expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return (IQueryable<TElement>)((IQueryProvider)this).CreateQuery(expression);
        }

        public object Execute(Expression expression)
        {
            return this
                .CallMethod(nameof(Execute))
                .WithTypeArgument(expression.Type)
                .WithArgument(expression)
                .Invoke<object>();
        }

        public TResult Execute<TResult>(Expression expression)
        {
            var itemType = expression.Type.UnwrapTypeParameter(typeof(IQueryable<>));

            var asyncEnumerable = this
                .CallMethod(nameof(ExecuteAsync))
                .WithTypeArgument(itemType)
                .WithArgument(expression)
                .WithArgument(CancellationToken.None)
                .Invoke<object>();

            var enumerable = GetType()
                .CallMethod(nameof(AsEnumerable))
                .WithTypeArgument(itemType)
                .WithArgument(asyncEnumerable)
                .Invoke<object>();

            return (TResult)enumerable;
        }

        public async IAsyncEnumerable<T> ExecuteAsync<T>(
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
                /* TODO: yield return objectBuilder.Build(values as IDictionary<string, object>);*/
                yield return default !;
            }
        }

        private static IEnumerable<T> AsEnumerable<T>(IAsyncEnumerable<T> asyncEnumerable)
        {
            return asyncEnumerable.AsEnumerable().Result;
        }
    }
}