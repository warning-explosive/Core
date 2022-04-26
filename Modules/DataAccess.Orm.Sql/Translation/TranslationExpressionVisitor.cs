namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Api.Model;
    using Api.Reading;
    using Basics;
    using Basics.Primitives;
    using Expressions;
    using Extensions;
    using Model;
    using Orm.Linq;
    using BinaryExpression = System.Linq.Expressions.BinaryExpression;
    using ConditionalExpression = System.Linq.Expressions.ConditionalExpression;
    using ConstantExpression = System.Linq.Expressions.ConstantExpression;
    using MethodCallExpression = System.Linq.Expressions.MethodCallExpression;
    using NewExpression = System.Linq.Expressions.NewExpression;
    using ParameterExpression = System.Linq.Expressions.ParameterExpression;
    using UnaryExpression = System.Linq.Expressions.UnaryExpression;

    internal class TranslationExpressionVisitor : ExpressionVisitor
    {
        private readonly IModelProvider _modelProvider;
        private readonly IExpressionTranslator _translator;
        private readonly IEnumerable<IMemberInfoTranslator> _memberInfoTranslators;

        private static readonly MethodInfo Select = LinqMethods.QueryableSelect();
        private static readonly MethodInfo Where = LinqMethods.QueryableWhere();
        private static readonly MethodInfo GroupBy2 = LinqMethods.QueryableGroupBy2();
        private static readonly MethodInfo GroupBy3 = LinqMethods.QueryableGroupBy3();
        private static readonly MethodInfo Single = LinqMethods.QueryableSingle();
        private static readonly MethodInfo SingleOrDefault = LinqMethods.QueryableSingleOrDefault();
        private static readonly MethodInfo First = LinqMethods.QueryableFirst();
        private static readonly MethodInfo FirstOrDefault = LinqMethods.QueryableFirstOrDefault();
        private static readonly MethodInfo Any = LinqMethods.QueryableAny();
        private static readonly MethodInfo All = LinqMethods.QueryableAll();
        private static readonly MethodInfo Count = LinqMethods.QueryableCount();
        private static readonly MethodInfo Contains = LinqMethods.QueryableContains();
        private static readonly MethodInfo Distinct = LinqMethods.QueryableDistinct();

        public TranslationExpressionVisitor(
            IModelProvider modelProvider,
            ExpressionTranslator translator,
            IEnumerable<IMemberInfoTranslator> memberInfoTranslators)
        {
            _modelProvider = modelProvider;
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

            var isQueryRoot = itemType.IsClass
              && itemType.IsSubclassOfOpenGeneric(typeof(IUniqueIdentified<>))
              && method == LinqMethods.All(itemType, itemType.UnwrapTypeParameter(typeof(IUniqueIdentified<>)));

            if (isQueryRoot)
            {
                Context.WithinConditionalScope(
                    Context.Parent is not JoinExpression,
                    action => Context.WithoutScopeDuplication(
                        () => new ProjectionExpression(itemType),
                        action),
                    () =>
                    {
                        Context.WithoutScopeDuplication(
                            () => new NamedSourceExpression(itemType, Context),
                            () => Context.WithinScope(
                                new QuerySourceExpression(itemType),
                                () =>
                                {
                                    base.VisitMethodCall(node);
                                    Context.ReverseLambdaParametersNames();
                                }));

                        SelectAll(Context.Parent!);
                    });

                return node;
            }

            var applyNamedSourceCondition = Context.Parent is JoinExpression
                 || (Context.Parent is not FilterExpression
                  && Context.Parent is not RowsFetchLimitExpression
                  && Context.Parent != null);

            if (method == Select)
            {
                Context.WithinConditionalScope(
                    applyNamedSourceCondition,
                    action => Context.WithoutScopeDuplication(
                        () => new NamedSourceExpression(itemType, Context),
                        action),
                    () => Context.WithoutScopeDuplication(
                        () => new ProjectionExpression(itemType),
                        () => BuildJoinExpression(Context, node, itemType)));

                return node;
            }

            if (method == Where)
            {
                Context.WithinConditionalScope(
                    applyNamedSourceCondition,
                    action => Context.WithoutScopeDuplication(
                        () => new NamedSourceExpression(itemType, Context),
                        action),
                    () => Context.WithoutScopeDuplication(
                        () => new FilterExpression(itemType),
                        () => BuildJoinExpression(Context, node, itemType)));

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

            if (method == Single
                || method == SingleOrDefault)
            {
                Context.WithinScope(
                    new RowsFetchLimitExpression(itemType, 2),
                    () => base.VisitMethodCall(node));

                return node;
            }

            if (method == First
                || method == FirstOrDefault)
            {
                Context.WithinScope(
                    new RowsFetchLimitExpression(itemType, 1),
                    () => base.VisitMethodCall(node));

                return node;
            }

            if (method == Any)
            {
                Context.WithinScope(
                    new ProjectionExpression(itemType),
                    () =>
                    {
                        // count(*) > 0 as "Any"
                        var countAllMethodCall = new Expressions.MethodCallExpression(
                            typeof(int),
                            nameof(Count),
                            null,
                            new[] { new SpecialExpression(itemType, "*") });

                        var binaryExpression = new Expressions.BinaryExpression(
                            typeof(bool),
                            BinaryOperator.GreaterThan,
                            countAllMethodCall,
                            new Expressions.ConstantExpression(typeof(int), 0));

                        Context.Apply(new NamedBindingExpression(method.Name, binaryExpression));

                        base.VisitMethodCall(node);
                    });

                return node;
            }

            if (method == All)
            {
                Context.WithinScope(
                    new ProjectionExpression(itemType),
                    () =>
                    {
                        Visit(node.Arguments[0]);

                        // count(case when <condition> then 1 else null end) = count(*) as "All"
                        Context.WithinScope(
                            new NamedBindingExpression(method.Name),
                            () =>
                            {
                                Context.WithinScope(
                                    new Expressions.BinaryExpression(typeof(bool), BinaryOperator.Equal),
                                    () =>
                                    {
                                        Context.WithinScope(
                                            new Expressions.MethodCallExpression(typeof(int), nameof(Count), null, Array.Empty<IIntermediateExpression>()),
                                            () =>
                                            {
                                                Context.WithinScope(
                                                    new Expressions.ConditionalExpression(typeof(int)),
                                                    () =>
                                                    {
                                                        Visit(node.Arguments[1]);
                                                        Context.Apply(new Expressions.ConstantExpression(typeof(int), 1));
                                                        Context.Apply(new Expressions.ConstantExpression(typeof(object), null));
                                                    });
                                            });

                                        var countAllMethodCall = new Expressions.MethodCallExpression(
                                            typeof(int),
                                            nameof(Count),
                                            null,
                                            new[] { new SpecialExpression(itemType, "*") });

                                        Context.Apply(countAllMethodCall);
                                    });
                            });
                    });

                return node;
            }

            if (method == Count)
            {
                Context.WithinScope(
                    new ProjectionExpression(itemType),
                    () =>
                    {
                        // count(*) as "Count"
                        var countAllMethodCall = new Expressions.MethodCallExpression(
                            typeof(int),
                            nameof(Count),
                            null,
                            new[] { new SpecialExpression(itemType, "*") });

                        Context.Apply(new NamedBindingExpression(method.Name, countAllMethodCall));

                        base.VisitMethodCall(node);
                    });

                return node;
            }

            if (method == Contains)
            {
                Context.WithinScope(new Expressions.BinaryExpression(typeof(bool), BinaryOperator.Contains),
                    () =>
                    {
                        Visit(node.Arguments[1]);

                        var subQueryExpression = ((IQueryable)((ConstantExpression)node.Arguments[0]).Value).Expression;

                        Context.Apply(_translator.Translate(subQueryExpression));
                    });

                return node;
            }

            if (method == Distinct)
            {
                Visit(node.Arguments[0]);
                var expression = Context.Expression ?? throw new InvalidOperationException("Unable to find distinct expression root");
                var projection = Context.GetProjectionExpression(expression) ?? throw new InvalidOperationException("Unable to find distinct projection");
                projection.IsDistinct = true;

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
            var expression = TryGetMemberInfoExpression(node.Member, out var recognized)
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
                    Context.WithinScope(new NamedBindingExpression(memberInfo.Name), () => Visit(argument));
                }
            }

            return node;
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            Context.WithinScope(new Expressions.ConditionalExpression(node.Type), () => base.VisitConditional(node));

            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            Context.WithinScope(new Expressions.BinaryExpression(node.Type, node.NodeType.AsBinaryOperator()), () => base.VisitBinary(node));

            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            var bypassedExpressionTypes = new[]
            {
                ExpressionType.Quote,
                ExpressionType.Convert,
                ExpressionType.ConvertChecked
            };

            if (bypassedExpressionTypes.Contains(node.NodeType))
            {
                base.VisitUnary(node);
            }
            else
            {
                Context.WithinScope(new Expressions.UnaryExpression(node.Type, node.NodeType.AsUnaryOperator()), () => base.VisitUnary(node));
            }

            return node;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            Context.WithinScope(Context.GetParameterExpression(node.Type), () => base.VisitParameter(node));

            return node;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
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

        private void SelectAll(IIntermediateExpression expression)
        {
            if (expression is not ProjectionExpression projection)
            {
                return;
            }

            if (!projection.IsProjectionToClass
                || projection.IsAnonymousProjection)
            {
                return;
            }

            var parameter = ExtractParameter(projection.Source, projection.Type);

            var flatProjectionColumns = _modelProvider
                .Columns(projection.Type)
                .Where(column => !column.IsMultipleRelation)
                .OrderBy(column => column.Name);

            foreach (var column in flatProjectionColumns)
            {
                Context.Apply(column.BuildExpression(parameter));
            }

            static Expressions.ParameterExpression ExtractParameter(IIntermediateExpression source, Type parameterType)
            {
                var parameterExpression = source
                    .ExtractParameters()
                    .OrderBy(parameter => parameter.Key)
                    .Select(parameter => parameter.Value)
                    .FirstOrDefault(parameter => parameter.Type == parameterType);

                if (parameterExpression == null)
                {
                    throw new InvalidOperationException("Unable to find appropriate parameter expression");
                }

                return parameterExpression;
            }
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

        private void BuildJoinExpression(TranslationContext context, MethodCallExpression node, Type itemType)
        {
            var relations = TranslationContext.ExtractRelations(
                node.Arguments[0].Type.UnwrapTypeParameter(typeof(IQueryable<>)),
                node.Arguments[1],
                _modelProvider);

            if (relations.Any())
            {
                using (var recursiveEnumerable = relations.MoveNext())
                {
                    context.WithoutScopeDuplication(() => new ProjectionExpression(itemType),
                        () =>
                        {
                            BuildJoinExpressionRecursive(Context, recursiveEnumerable, () => Visit(node.Arguments[0]));

                            SelectAll(Context.Parent!);
                        });

                    Visit(node.Arguments[1]);
                }
            }
            else
            {
                base.VisitMethodCall(node);
            }

            static void BuildJoinExpressionRecursive(
                TranslationContext context,
                IRecursiveEnumerable<Relation> recursiveEnumerable,
                Action? action)
            {
                if (recursiveEnumerable.TryMoveNext(out var relation))
                {
                    context.WithinScope(new JoinExpression(),
                        () =>
                        {
                            context.WithoutScopeDuplication(() => new NamedSourceExpression(relation.Target, context),
                                () => context.Apply(new QuerySourceExpression(relation.Target)));

                            BuildJoinExpressionRecursive(context, recursiveEnumerable, action);

                            BuildJoinOnExpression(context, relation);
                        });
                }
                else
                {
                    action?.Invoke();
                }

                static void BuildJoinOnExpression(TranslationContext context, Relation relation)
                {
                    context.Apply(new Expressions.BinaryExpression(
                        typeof(bool),
                        BinaryOperator.Equal,
                        new SimpleBindingExpression(
                            relation.Target.GetProperty(nameof(IUniqueIdentified.PrimaryKey)),
                            typeof(Guid),
                            context.GetParameterExpression(relation.Target)),
                        new SimpleBindingExpression(
                            relation.Target.GetProperty(nameof(IUniqueIdentified.PrimaryKey)),
                            typeof(Guid),
                            new SimpleBindingExpression(
                                relation.Property.Reflected,
                                relation.Target,
                                context.GetParameterExpression(relation.Source)))));
                }
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
                    TranslationContext.ExtractLambdaExpression(node, selector));
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
                    TranslationContext.ExtractLambdaExpression(node, selector));
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

                static IEnumerable<IIntermediateExpression> GetBindings(
                    ProjectionExpression projection,
                    Expressions.ParameterExpression parameterExpression)
                {
                    var bindings = projection.IsProjectionToClass
                        ? projection.Bindings.Select(NamedBindingExpression.Unwrap).Select(expression => expression.ReplaceParameter(parameterExpression))
                        : projection.Bindings.Select(expression => expression.ReplaceParameter(parameterExpression));

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
                            BinaryOperator.Equal,
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