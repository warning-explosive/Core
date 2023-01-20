namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Transaction;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot;

    [Component(EnLifestyle.Singleton)]
    internal class QueryProvider : IAsyncQueryProvider,
                                   IResolvable<IAsyncQueryProvider>,
                                   IResolvable<IQueryProvider>
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IQueryTranslator _translator;
        private readonly IQueryMaterializerComposite _materializer;

        public QueryProvider(
            IDependencyContainer dependencyContainer,
            IQueryTranslator translator,
            IQueryMaterializerComposite materializer)
        {
            _dependencyContainer = dependencyContainer;
            _translator = translator;
            _materializer = materializer;
        }

        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            var itemType = expression.Type.ExtractGenericArgumentAtOrSelf(typeof(IQueryable<>));

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
            var itemType = expression.Type.ExtractGenericArgumentAtOrSelf(typeof(IQueryable<>));

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
            var transaction = _dependencyContainer.Resolve<IAdvancedDatabaseTransaction>();

            var query = _translator.Translate(expression);

            var item = await _materializer
                .MaterializeScalar(transaction, query, typeof(T), token)
                .ConfigureAwait(false);

            return (T)item!;
        }

        public async IAsyncEnumerable<T> ExecuteAsync<T>(Expression expression, [EnumeratorCancellation] CancellationToken token)
        {
            var transaction = _dependencyContainer.Resolve<IAdvancedDatabaseTransaction>();

            var query = _translator.Translate(expression);

            var source = _materializer
                .Materialize(transaction, query, typeof(T), token)
                .WithCancellation(token)
                .ConfigureAwait(false);

            await foreach (var item in source)
            {
                yield return (T)item!;
            }
        }

        private static T AsScalar<T>(Task<T> task)
        {
            return task.Result;
        }

        private static IEnumerable<T> AsEnumerable<T>(IAsyncEnumerable<T> asyncEnumerable, CancellationToken token)
        {
            return asyncEnumerable.AsEnumerable(token).Result;
        }
    }
}