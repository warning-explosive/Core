namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using Abstractions;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;

    [Component(EnLifestyle.Scoped)]
    internal class QueryProvider : IAsyncQueryProvider,
                                   IExternalResolvable<IQueryProvider>
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IQueryTranslator _translator;

        public QueryProvider(IDependencyContainer dependencyContainer, IQueryTranslator translator)
        {
            _dependencyContainer = dependencyContainer;
            _translator = translator;
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
                .WithArgument(CancellationToken.None)
                .Invoke<object>();

            return (TResult)enumerable;
        }

        public async IAsyncEnumerable<T> ExecuteAsync<T>(
            Expression expression,
            [EnumeratorCancellation] CancellationToken token)
        {
            var query = await _translator.Translate(expression, token).ConfigureAwait(false);

            await foreach (var item in Materialize<T>(query, token))
            {
                yield return item;
            }
        }

        private IAsyncEnumerable<T> Materialize<T>(IQuery query, CancellationToken token)
        {
            return this
                .CallMethod(nameof(Materialize))
                .WithTypeArgument(query.GetType())
                .WithTypeArgument<T>()
                .WithArgument(query)
                .WithArgument(token)
                .Invoke<IAsyncEnumerable<T>>();
        }

        private IAsyncEnumerable<TItem> Materialize<TQuery, TItem>(TQuery query, CancellationToken token)
            where TQuery : IQuery
        {
            return _dependencyContainer
                .Resolve<IQueryMaterializer<TQuery, TItem>>()
                .Materialize(query, token);
        }

        private static IEnumerable<T> AsEnumerable<T>(IAsyncEnumerable<T> asyncEnumerable, CancellationToken token)
        {
            return asyncEnumerable.AsEnumerable(token);
        }
    }
}