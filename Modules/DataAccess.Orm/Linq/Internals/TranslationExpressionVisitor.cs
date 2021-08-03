namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Abstractions;
    using Basics;
    using Basics.Primitives;
    using Contract.Abstractions;
    using Expressions;
    using GenericDomain.Abstractions;
    using BinaryExpression = System.Linq.Expressions.BinaryExpression;
    using ConditionalExpression = System.Linq.Expressions.ConditionalExpression;
    using MethodCallExpression = System.Linq.Expressions.MethodCallExpression;
    using NewExpression = System.Linq.Expressions.NewExpression;
    using ParameterExpression = System.Linq.Expressions.ParameterExpression;

    internal class TranslationExpressionVisitor : ExpressionVisitor
    {
        private const string QueryParameterFormat = "param_{0}";

        private readonly ExpressionTranslator _translator;
        private readonly IEnumerable<IMemberInfoTranslator> _memberInfoTranslators;

        private readonly TranslationContext _translationContext;

        private readonly Stack<IIntermediateExpression> _stack;

        private static readonly MethodInfo Select = LinqMethods.QueryableSelect();
        private static readonly MethodInfo Where = LinqMethods.QueryableWhere();
        private static readonly MethodInfo GroupBy2 = LinqMethods.QueryableGroupBy2();

        private uint _queryParameterIndex;

        public TranslationExpressionVisitor(
            ExpressionTranslator translator,
            IEnumerable<IMemberInfoTranslator> memberInfoTranslators)
        {
            _translator = translator;
            _memberInfoTranslators = memberInfoTranslators;

            _translationContext = new TranslationContext(this);

            _stack = new Stack<IIntermediateExpression>();

            _queryParameterIndex = 0;
        }

        internal IIntermediateExpression? Expression { get; private set; }

        internal string NextQueryParameterName()
        {
            return string.Format(QueryParameterFormat, _queryParameterIndex++);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var method = node.Method.IsGenericMethod
                ? node.Method.GetGenericMethodDefinition()
                : node.Method;

            var itemType = node.Type.UnwrapTypeParameter(typeof(IQueryable<>));

            if (itemType.IsClass
                && typeof(IEntity).IsAssignableFrom(itemType)
                && method == LinqMethods.All(itemType))
            {
                if (_stack.TryPeek(out var outer)
                    && outer is ProjectionExpression)
                {
                    WithScopeOpening(new QuerySourceExpression(itemType), () => base.VisitMethodCall(node));

                    return node;
                }

                var projection = new ProjectionExpression(itemType, new QuerySourceExpression(itemType), Enumerable.Empty<IIntermediateExpression>());

                WithScopeOpening(projection, () => base.VisitMethodCall(node));

                return node;
            }

            if (method == Select)
            {
                WithScopeOpening(new ProjectionExpression(itemType), () => base.VisitMethodCall(node));

                return node;
            }

            if (method == Where)
            {
                WithoutScopeDuplication(() => new FilterExpression(itemType), () => base.VisitMethodCall(node));

                return node;
            }

            if (method == GroupBy2)
            {
                var sourceType = node
                    .Arguments[0]
                    .Type
                    .ExtractGenericArgumentsAt(typeof(IQueryable<>))
                    .Single();

                var typeArguments = itemType
                    .ExtractGenericArguments(typeof(IGrouping<,>))
                    .Single();

                var keyType = typeArguments[0];
                var valueType = typeArguments[1];

                var sourceExpression = node.Arguments[0];

                var keyExpression = new ExtractLambdaExpressionVisitor().Extract(node.Arguments[1])
                                    ?? throw new NotSupportedException($"method: {node.Method}");

                var groupBy = new GroupByExpression(itemType);

                var keysProjection = new ProjectionExpression(keyType) { IsDistinct = true };

                WithScopeOpening(groupBy,
                    () => WithScopeOpening(keysProjection,
                        () =>
                        {
                            Visit(sourceExpression);
                            VisitLambda(sourceType, keyType, keyExpression);
                        }));

                var sourceFilter = new FilterExpression(sourceType);

                WithScopeOpening(groupBy,
                    () => WithScopeOpening(sourceFilter,
                        () =>
                        {
                            Visit(sourceExpression);

                            var parameter = new Expressions.ParameterExpression(keysProjection.ItemType, "a");

                            foreach (var filterBinding in keysProjection.GetFilterBindings(_translationContext, parameter))
                            {
                                Apply(filterBinding);
                            }

                            Apply(parameter);
                        }));

                return node;
            }

            if (TryGetMemberInfoExpression(node.Method, out var recognized))
            {
                WithScopeOpening(recognized, () => base.VisitMethodCall(node));

                return node;
            }

            throw new NotSupportedException($"method: {node.Method}");
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            IIntermediateExpression expression = TryGetMemberInfoExpression(node.Member, out var recognized)
                ? recognized
                : new SimpleBindingExpression(node.Type, node.Member.Name);

            WithScopeOpening(expression, () => base.VisitMember(node));

            return node;
        }

        protected override Expression VisitNew(NewExpression node)
        {
            WithoutScopeOpening(() => new Expressions.NewExpression(node.Type),
                () =>
                {
                    node.Members
                        .Zip(node.Arguments, (memberInfo, argument) =>
                        {
                            var expression = _translator.Translate(argument);
                            return (memberInfo, expression);
                        })
                        .Each(pair =>
                        {
                            if (pair.expression is INamedIntermediateExpression namedExpression
                                && !namedExpression.Name.Equals(pair.memberInfo.Name, StringComparison.OrdinalIgnoreCase))
                            {
                                Apply(new NamedBindingExpression(pair.expression, pair.memberInfo.Name));
                            }
                            else
                            {
                                Apply(pair.expression);
                            }
                        });
                });

            return node;
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            WithScopeOpening(new Expressions.ConditionalExpression(node.Type), () => base.VisitConditional(node));

            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            WithScopeOpening(new Expressions.BinaryExpression(node.Type, node.NodeType), () => base.VisitBinary(node));

            return node;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            WithScopeOpening(new Expressions.ParameterExpression(node.Type, node.Name), () => base.VisitParameter(node));

            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Type.IsSubclassOfOpenGeneric(typeof(IReadRepository<>)))
            {
                return base.VisitConstant(node);
            }

            WithScopeOpening(new QueryParameterExpression(node.Type, NextQueryParameterName(), node.Value), () => base.VisitConstant(node));

            return node;
        }

        private void WithScopeOpening<T>(T expression, Action? action = null)
            where T : class, IIntermediateExpression
        {
            using (Disposable.Create(_stack, Push, Pop))
            {
                action?.Invoke();
            }

            void Push(Stack<IIntermediateExpression> stack)
            {
                stack.Push(expression);
            }

            void Pop(Stack<IIntermediateExpression> stack)
            {
                var expr = (T)stack.Pop();

                if (_stack.TryPeek(out var outer))
                {
                    Apply(expr, outer);
                }
                else
                {
                    Expression = expr;
                }
            }
        }

        private void WithoutScopeDuplication<T>(Func<T> intermediateExpressionProducer, Action? action = null)
            where T : class, IIntermediateExpression
        {
            if (_stack.TryPeek(out var outer)
                && outer is T)
            {
                action?.Invoke();
            }

            WithScopeOpening(intermediateExpressionProducer(), action);
        }

        private void WithoutScopeOpening<T>(Func<T> intermediateExpressionProducer, Action? action = null)
            where T : class, IIntermediateExpression
        {
            action?.Invoke();

            if (_stack.TryPeek(out var outer))
            {
                Apply(intermediateExpressionProducer(), outer);
            }
        }

        private void Apply(IIntermediateExpression expression)
        {
            if (_stack.TryPeek(out var outer))
            {
                Apply(expression, outer);
            }
        }

        private void Apply(IIntermediateExpression inner, IIntermediateExpression outer)
        {
            var service = typeof(IApplicable<>).MakeGenericType(inner.GetType());

            if (outer.IsInstanceOfType(service))
            {
                outer
                    .CallMethod(nameof(IApplicable<IIntermediateExpression>.Apply))
                    .WithArgument(_translationContext)
                    .WithArgument(inner)
                    .Invoke();
            }
            else
            {
                throw new InvalidOperationException($"Could not apply {inner.GetType().Name} for {outer.GetType().Name}");
            }
        }

        private bool TryGetMemberInfoExpression(MemberInfo memberInfo, [NotNullWhen(true)] out IIntermediateExpression? expression)
        {
            var context = new MemberTranslationContext(memberInfo, this);

            expression = _memberInfoTranslators
                .Select(provider =>
                {
                    var success = provider.TryRecognize(context, out var info);
                    return (success, info);
                })
                .Where(pair => pair.success)
                .Select(pair => pair.info)
                .InformativeSingleOrDefault(Amb);

            return expression != null;

            string Amb(IEnumerable<IIntermediateExpression?> infos)
            {
                throw new InvalidOperationException($"More than one expression suitable for {memberInfo.DeclaringType}.{memberInfo.Name} member");
            }
        }

        private void VisitLambda(Type sourceType, Type itemType, LambdaExpression lambdaExpression)
        {
            this.CallMethod(nameof(VisitLambdaGeneric))
                .WithTypeArgument(sourceType)
                .WithTypeArgument(itemType)
                .WithArgument(lambdaExpression)
                .Invoke();
        }

        private void VisitLambdaGeneric<TSource, TItem>(Expression<Func<TSource, TItem>> expression)
        {
            _ = VisitLambda(expression);
        }
    }
}