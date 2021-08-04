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
    using Contract.Abstractions;
    using Expressions;
    using GenericDomain.Abstractions;
    using BinaryExpression = System.Linq.Expressions.BinaryExpression;
    using ConditionalExpression = System.Linq.Expressions.ConditionalExpression;
    using ConstantExpression = System.Linq.Expressions.ConstantExpression;
    using MethodCallExpression = System.Linq.Expressions.MethodCallExpression;
    using NewExpression = System.Linq.Expressions.NewExpression;
    using ParameterExpression = System.Linq.Expressions.ParameterExpression;

    internal class TranslationExpressionVisitor : ExpressionVisitor
    {
        private readonly ExpressionTranslator _translator;
        private readonly IEnumerable<IMemberInfoTranslator> _memberInfoTranslators;

        private static readonly MethodInfo Select = LinqMethods.QueryableSelect();
        private static readonly MethodInfo Where = LinqMethods.QueryableWhere();
        private static readonly MethodInfo GroupBy2 = LinqMethods.QueryableGroupBy2();

        public TranslationExpressionVisitor(
            ExpressionTranslator translator,
            IEnumerable<IMemberInfoTranslator> memberInfoTranslators)
        {
            _translator = translator;
            _memberInfoTranslators = memberInfoTranslators;

            Context = new TranslationContext();
        }

        internal TranslationContext Context { get; }

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
                Context.WithoutExpressionScopeDuplication(() => new ProjectionExpression(itemType),
                    () => Context.WithinExpressionScope(new NamedSourceExpression(itemType, new QuerySourceExpression(itemType), Context.GetParameterExpression(itemType)),
                        () => base.VisitMethodCall(node)));

                return node;
            }

            if (method == Select)
            {
                Context.WithinExpressionScope(new ProjectionExpression(itemType), () => base.VisitMethodCall(node));

                return node;
            }

            if (method == Where)
            {
                Context.WithoutExpressionScopeDuplication(() => new FilterExpression(itemType), () => base.VisitMethodCall(node));

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

                Context.WithinExpressionScope(groupBy,
                    () => Context.WithinExpressionScope(keysProjection,
                        () =>
                        {
                            Visit(sourceExpression);
                            VisitLambda(sourceType, keyType, keyExpression);
                        }));

                var sourceFilter = new FilterExpression(sourceType);

                Context.WithinExpressionScope(groupBy,
                    () => Context.WithinExpressionScope(sourceFilter,
                        () =>
                        {
                            Visit(sourceExpression);

                            var parameter = Context.GetParameterExpression(keysProjection.Type);

                            foreach (var filterBinding in keysProjection.GetFilterBindings(Context, parameter))
                            {
                                Context.Apply(filterBinding);
                            }

                            Context.Apply(parameter);
                        }));

                return node;
            }

            if (TryGetMemberInfoExpression(node.Method, out var recognized))
            {
                Context.WithinExpressionScope(recognized, () => base.VisitMethodCall(node));

                return node;
            }

            throw new NotSupportedException($"method: {node.Method}");
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            IIntermediateExpression expression = TryGetMemberInfoExpression(node.Member, out var recognized)
                ? recognized
                : new SimpleBindingExpression(node.Type, node.Member.Name);

            Context.WithinExpressionScope(expression, () => base.VisitMember(node));

            return node;
        }

        protected override Expression VisitNew(NewExpression node)
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
                        Context.Apply(new NamedBindingExpression(pair.expression, pair.memberInfo.Name));
                    }
                    else
                    {
                        Context.Apply(pair.expression);
                    }
                });

            Context.Apply(new Expressions.NewExpression(node.Type));

            return node;
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            Context.WithinExpressionScope(new Expressions.ConditionalExpression(node.Type), () => base.VisitConditional(node));

            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            Context.WithinExpressionScope(new Expressions.BinaryExpression(node.Type, node.NodeType), () => base.VisitBinary(node));

            return node;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            Context.WithinExpressionScope(Context.GetParameterExpression(node.Type), () => base.VisitParameter(node));

            return node;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            Context.WithinScope<ProjectionExpression>(() => Context.Apply(Context.GetParameterExpression(node.Parameters.Single().Type)));

            Visit(node.Body);

            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Type.IsSubclassOfOpenGeneric(typeof(IReadRepository<>)))
            {
                return base.VisitConstant(node);
            }

            Context.WithinExpressionScope(QueryParameterExpression.Create(Context, node.Type, node.Value), () => base.VisitConstant(node));

            return node;
        }

        private bool TryGetMemberInfoExpression(MemberInfo memberInfo, [NotNullWhen(true)] out IIntermediateExpression? expression)
        {
            var context = new MemberTranslationContext(memberInfo);

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