namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions.Container;

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

            var isScalar = typeof(TResult) == itemType;

            if (isScalar)
            {
                var task = this
                    .CallMethod(nameof(ExecuteScalarAsync))
                    .WithTypeArgument(itemType)
                    .WithArgument(expression)
                    .WithArgument(CancellationToken.None)
                    .Invoke<Task>();

                var scalar = GetType()
                    .CallMethod(nameof(AsScalar))
                    .WithTypeArgument(itemType)
                    .WithArgument(task)
                    .Invoke();

                return (TResult)scalar!;
            }

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
                .Invoke();

            return (TResult)enumerable!;
        }

        public async Task<T> ExecuteScalarAsync<T>(Expression expression, CancellationToken token)
        {
            var query = _translator.Translate(expression);

            return await MaterializeScalar<T>(query, token).ConfigureAwait(false);
        }

        public async IAsyncEnumerable<T> ExecuteAsync<T>(Expression expression, [EnumeratorCancellation] CancellationToken token)
        {
            var query = _translator.Translate(expression);

            await foreach (var item in Materialize<T>(query, token))
            {
                yield return item;
            }
        }

        private Task<T> MaterializeScalar<T>(IQuery query, CancellationToken token)
        {
            return _dependencyContainer
                .ResolveGeneric(typeof(IQueryMaterializer<,>), query.GetType(), typeof(T))
                .CallMethod(nameof(IQueryMaterializer<IQuery, T>.MaterializeScalar))
                .WithArguments(query, token)
                .Invoke<Task<T>>();
        }

        private static T AsScalar<T>(Task<T> task)
        {
            return task.Result;
        }

        private IAsyncEnumerable<T> Materialize<T>(IQuery query, CancellationToken token)
        {
            return _dependencyContainer
                .ResolveGeneric(typeof(IQueryMaterializer<,>), query.GetType(), typeof(T))
                .CallMethod(nameof(IQueryMaterializer<IQuery, T>.Materialize))
                .WithArguments(query, token)
                .Invoke<IAsyncEnumerable<T>>();
        }

        private static IEnumerable<T> AsEnumerable<T>(IAsyncEnumerable<T> asyncEnumerable, CancellationToken token)
        {
            return asyncEnumerable.AsEnumerable(token);
        }
    }
}