namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot;
    using Transaction;

    [Component(EnLifestyle.Singleton)]
    internal class QueryProvider : IAsyncQueryProvider,
                                   IResolvable<IAsyncQueryProvider>,
                                   IResolvable<IQueryProvider>
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IExpressionTranslator _translator;
        private readonly ICommandMaterializerComposite _materializer;

        public QueryProvider(
            IDependencyContainer dependencyContainer,
            IExpressionTranslator translator,
            ICommandMaterializerComposite materializer)
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

            {
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
        }

        public async Task<T> ExecuteScalarAsync<T>(Expression expression, CancellationToken token)
        {
            var transaction = _dependencyContainer.Resolve<IAdvancedDatabaseTransaction>();

            var command = _translator.Translate(expression);

            var asyncSource = _materializer
                .Materialize<T>(transaction, command, token)
                .WithCancellation(token)
                .ConfigureAwait(false);

            var buffer = new List<T>();

            await foreach (var item in asyncSource)
            {
                buffer.Add(item);
            }

            return buffer.SingleOrDefault();
        }

        public IAsyncEnumerable<T> ExecuteAsync<T>(Expression expression, CancellationToken token)
        {
            var transaction = _dependencyContainer.Resolve<IAdvancedDatabaseTransaction>();

            var command = _translator.Translate(expression);

            return _materializer.Materialize<T>(transaction, command, token);
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