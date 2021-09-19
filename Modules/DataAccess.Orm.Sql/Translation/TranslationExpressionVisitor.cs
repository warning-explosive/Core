namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Api.DatabaseEntity;
    using Api.Reading;
    using Basics;
    using Expressions;
    using BinaryExpression = System.Linq.Expressions.BinaryExpression;
    using ConditionalExpression = System.Linq.Expressions.ConditionalExpression;
    using ConstantExpression = System.Linq.Expressions.ConstantExpression;
    using MethodCallExpression = System.Linq.Expressions.MethodCallExpression;
    using NewExpression = System.Linq.Expressions.NewExpression;
    using ParameterExpression = System.Linq.Expressions.ParameterExpression;

    internal class TranslationExpressionVisitor : ExpressionVisitor
    {
        private readonly IExpressionTranslator _translator;
        private readonly IEnumerable<IMemberInfoTranslator> _memberInfoTranslators;

        private static readonly MethodInfo Select = LinqMethods.QueryableSelect();
        private static readonly MethodInfo Where = LinqMethods.QueryableWhere();
        private static readonly MethodInfo GroupBy2 = LinqMethods.QueryableGroupBy2();
        private static readonly MethodInfo GroupBy3 = LinqMethods.QueryableGroupBy3();

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
                && itemType.IsSubclassOfOpenGeneric(typeof(IDatabaseEntity<>))
                && method == LinqMethods.All(itemType, itemType.ExtractGenericArgumentsAt(typeof(IDatabaseEntity<>)).Single()))
            {
                Context.WithoutScopeDuplication(() => new ProjectionExpression(itemType),
                    () => Context.WithinScope(new NamedSourceExpression(itemType, new QuerySourceExpression(itemType), Context.GetParameterExpression(itemType)),
                        () => base.VisitMethodCall(node)));

                return node;
            }

            if (method == Select)
            {
                Context.WithinScope(new ProjectionExpression(itemType), () => base.VisitMethodCall(node));

                return node;
            }

            if (method == Where)
            {
                Context.WithoutScopeDuplication(() => new FilterExpression(itemType), () => base.VisitMethodCall(node));

                return node;
            }

            if (method == GroupBy2)
            {
                GroupBy(node, itemType, false);

                return node;
            }

            if (method == GroupBy3)
            {
                GroupBy(node, itemType, true);

                return node;
            }

            if (TryGetMemberInfoExpression(node.Method, out var recognized))
            {
                Context.WithinScope(recognized, () => base.VisitMethodCall(node));

                return node;
            }

            throw new NotSupportedException($"method: {node.Method}");
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            IIntermediateExpression expression = TryGetMemberInfoExpression(node.Member, out var recognized)
                ? recognized
                : new SimpleBindingExpression(node.Member, node.Type);

            Context.WithinScope(expression, () => base.VisitMember(node));

            return node;
        }

        protected override Expression VisitNew(NewExpression node)
        {
            Context.Apply(new Expressions.NewExpression(node.Type));

            foreach (var (memberInfo, argument) in node.Members.Zip(node.Arguments, (memberInfo, argument) => (memberInfo, argument)))
            {
                if (argument is MemberExpression memberExpression
                    && memberExpression.Member.MemberType == MemberTypes.Property
                    && memberExpression.Member.Name.Equals(memberInfo.Name, StringComparison.OrdinalIgnoreCase))
                {
                    Visit(argument);
                }
                else
                {
                    Context.WithinScope(new NamedBindingExpression(memberInfo), () => Visit(argument));
                }
            }

            Context.Apply(new Expressions.NewExpression(node.Type));

            return node;
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            Context.WithinScope(new Expressions.ConditionalExpression(node.Type), () => base.VisitConditional(node));

            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            Context.WithinScope(new Expressions.BinaryExpression(node.Type, node.NodeType), () => base.VisitBinary(node));

            return node;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            Context.WithinScope(Context.GetParameterExpression(node.Type), () => base.VisitParameter(node));

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
            if (node.Type.IsSubclassOfOpenGeneric(typeof(IReadRepository<,>)))
            {
                return base.VisitConstant(node);
            }

            Context.WithinScope(QueryParameterExpression.Create(Context, node.Type, node.Value), () => base.VisitConstant(node));

            return node;
        }

        private bool TryGetMemberInfoExpression(
            MemberInfo memberInfo,
            [NotNullWhen(true)] out IIntermediateExpression? expression)
        {
            var context = new MemberTranslationContext(Context, memberInfo);

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

        private void GroupBy(MethodCallExpression node, Type itemType, bool isGroupBy3)
        {
            var sourceType = node.Arguments[0].Type.UnwrapTypeParameter(typeof(IQueryable<>));
            var typeArguments = itemType.UnwrapTypeParameters(typeof(IGrouping<,>));

            var keyType = typeArguments[0];
            var valueType = typeArguments[1];

            var sourceExpression = node.Arguments[0];

            var keyExpression = MakeSelectExpression(node, sourceExpression, sourceType, keyType, node.Arguments[1]);

            var keyProjection = (ProjectionExpression)_translator.Translate(keyExpression);
            keyProjection.IsDistinct = true;

            var producer = ValuesExpressionProducer(
                _translator,
                Context.Clone(),
                node,
                keyProjection,
                sourceExpression,
                sourceType,
                valueType,
                isGroupBy3);

            var groupBy = new GroupByExpression(itemType, producer);

            Context.WithinScope(groupBy, () => Context.Apply(keyProjection));

            static LambdaExpression ExtractLambdaExpression(MethodCallExpression node, Expression selector)
            {
                return new ExtractLambdaExpressionVisitor().Extract(selector)
                       ?? throw new NotSupportedException($"method: {node.Method}");
            }

            static MethodCallExpression MakeSelectExpression(
                MethodCallExpression node,
                Expression sourceExpression,
                Type sourceType,
                Type targetType,
                Expression selector)
            {
                return Expression.Call(null,
                    Select.MakeGenericMethod(sourceType, targetType),
                    sourceExpression,
                    ExtractLambdaExpression(node, selector));
            }

            static MethodCallExpression MakeWhereExpression(
                MethodCallExpression node,
                Expression sourceExpression,
                Type sourceType,
                Expression selector)
            {
                return Expression.Call(null,
                    Where.MakeGenericMethod(sourceType),
                    sourceExpression,
                    ExtractLambdaExpression(node, selector));
            }

            static LambdaExpression MakePredicate(
                TranslationContext context,
                Type sourceType,
                ProjectionExpression keyExpression,
                IReadOnlyDictionary<string, object?> keyValues)
            {
                Expressions.ParameterExpression parameterExpression = context.GetParameterExpression(sourceType);

                var body = GetBindings(keyExpression, parameterExpression)
                    .Select(BindingFilter(context, keyExpression, keyValues))
                    .Select(expression => expression.AsExpressionTree())
                    .Aggregate((left, right) => Expression.MakeBinary(ExpressionType.AndAlso, left, right));

                return Expression.Lambda(body, (ParameterExpression)parameterExpression.AsExpressionTree());

                static IEnumerable<IIntermediateExpression> GetBindings(ProjectionExpression projection, Expressions.ParameterExpression parameterExpression)
                {
                    var visitor = new ReplaceParameterVisitor(parameterExpression);

                    IEnumerable<IIntermediateExpression> bindings = projection.IsProjectionToClass switch
                    {
                        true when !projection.Bindings.Any() => projection.Type.SelectAll(parameterExpression),
                        true => projection.Bindings.Select(NamedBindingExpression.Unwrap).Select(visitor.Visit),
                        _ => projection.Bindings.Select(visitor.Visit)
                    };

                    return bindings;
                }

                static Func<IIntermediateExpression, Expressions.BinaryExpression> BindingFilter(
                    TranslationContext context,
                    ProjectionExpression keyExpression,
                    IReadOnlyDictionary<string, object?> keyValues)
                {
                    return expression =>
                    {
                        var value = keyExpression.IsProjectionToClass
                                    && expression is IBindingIntermediateExpression binding
                            ? keyValues[binding.Name]
                            : keyValues.Single().Value;

                        return new Expressions.BinaryExpression(
                            typeof(bool),
                            ExpressionType.Equal,
                            expression,
                            QueryParameterExpression.Create(context, expression.Type, value, true));
                    };
                }
            }

            static Func<IReadOnlyDictionary<string, object?>, IIntermediateExpression> ValuesExpressionProducer(
                IExpressionTranslator translator,
                TranslationContext context,
                MethodCallExpression node,
                ProjectionExpression keyProjection,
                Expression sourceExpression,
                Type sourceType,
                Type valueType,
                bool isGroupBy3)
            {
                return keyValues =>
                {
                    var valuePredicate = MakePredicate(context, sourceType, keyProjection, keyValues);

                    var valueExpression = MakeWhereExpression(node, sourceExpression, sourceType, valuePredicate);

                    if (isGroupBy3)
                    {
                        valueExpression = MakeSelectExpression(node, valueExpression, sourceType, valueType, node.Arguments[2]);
                    }

                    return translator.Translate(valueExpression);
                };
            }
        }
    }
}