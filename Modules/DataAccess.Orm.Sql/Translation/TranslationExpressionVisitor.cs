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
    using Basics.Enumerations;
    using Basics.Primitives;
    using Expressions;
    using Model;
    using Orm.Linq;
    using BinaryExpression = System.Linq.Expressions.BinaryExpression;
    using ConditionalExpression = System.Linq.Expressions.ConditionalExpression;
    using MethodCallExpression = System.Linq.Expressions.MethodCallExpression;
    using NewExpression = System.Linq.Expressions.NewExpression;
    using ParameterExpression = System.Linq.Expressions.ParameterExpression;
    using UnaryExpression = System.Linq.Expressions.UnaryExpression;

    internal class TranslationExpressionVisitor : ExpressionVisitor
    {
        private readonly TranslationContext _context;
        private readonly IModelProvider _modelProvider;
        private readonly ILinqExpressionPreprocessorComposite _preprocessor;
        private readonly IEnumerable<IMemberInfoTranslator> _memberInfoTranslators;

        public TranslationExpressionVisitor(
            TranslationContext translationContext,
            IModelProvider modelProvider,
            ILinqExpressionPreprocessorComposite preprocessor,
            IEnumerable<IMemberInfoTranslator> memberInfoTranslators)
        {
            _context = translationContext;
            _modelProvider = modelProvider;
            _preprocessor = preprocessor;
            _memberInfoTranslators = memberInfoTranslators;
        }

        public SqlExpression Translate(Expression expression)
        {
            Visit(expression);
            return new SqlExpression(
                _context.Expression.EnsureNotNull("Sql expression wasn't built"),
                _context.BuildCommandParametersExtractor(_preprocessor));
        }

        public sealed override Expression Visit(Expression node)
        {
            using (Disposable.Create(node, _context.PushPath, _context.PopPath))
            {
                return base.Visit(node);
            }
        }

        [SuppressMessage("Analysis", "CA1502", Justification = "complex expression visitor")]
        [SuppressMessage("Analysis", "CA1506", Justification = "complex expression visitor")]
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var method = node.Method.GenericMethodDefinitionOrSelf();

            var itemType = node.Type.ExtractGenericArgumentAtOrSelf(typeof(IQueryable<>));

            if (method.IsQueryRoot())
            {
                _context.WithinConditionalScope(
                    parent => parent is not JoinExpression,
                    action => _context.WithoutScopeDuplication(
                        () => new ProjectionExpression(itemType),
                        action),
                    () =>
                    {
                        _context.WithoutScopeDuplication(
                            () => new NamedSourceExpression(itemType, _context),
                            () => _context.WithinScope(
                                new QuerySourceExpression(itemType),
                                () =>
                                {
                                    base.VisitMethodCall(node);
                                    _context.ReverseLambdaParametersNames();
                                }));

                        SelectAll(_context.Parent!);
                    });

                return node;
            }

            if (method == LinqMethods.QueryableSelect())
            {
                _context.WithinConditionalScope(
                    parent => parent is ProjectionExpression || parent is JoinExpression,
                    action => _context.WithoutScopeDuplication(
                        () => new NamedSourceExpression(itemType, _context),
                        action),
                    () => _context.WithoutScopeDuplication(
                        () => new ProjectionExpression(itemType),
                        () =>
                        {
                            var bindings = new[] { node.Arguments[1] };

                            if (TryBuildJoinExpression(_context, node.Arguments[0], bindings, itemType))
                            {
                                Visit(bindings[0]);
                            }
                            else
                            {
                                base.VisitMethodCall(node);
                            }
                        }));

                return node;
            }

            if (method == LinqMethods.QueryableWhere())
            {
                _context.WithinConditionalScope(
                    parent => parent is ProjectionExpression || parent is JoinExpression,
                    action => _context.WithoutScopeDuplication(
                        () => new NamedSourceExpression(itemType, _context),
                        action),
                    () => _context.WithoutScopeDuplication(
                        () => new FilterExpression(itemType),
                        () =>
                        {
                            var bindings = new[] { node.Arguments[1] };

                            if (TryBuildJoinExpression(_context, node.Arguments[0], bindings, itemType))
                            {
                                Visit(bindings[0]);
                            }
                            else
                            {
                                base.VisitMethodCall(node);
                            }
                        }));

                return node;
            }

            if (method == LinqMethods.QueryableOrderBy()
                || method == LinqMethods.QueryableOrderByDescending()
                || method == LinqMethods.QueryableThenBy()
                || method == LinqMethods.QueryableThenByDescending())
            {
                _context.WithinConditionalScope(
                    parent => parent is ProjectionExpression || parent is JoinExpression,
                    action => _context.WithoutScopeDuplication(
                        () => new NamedSourceExpression(itemType, _context),
                        action),
                    () => _context.WithoutScopeDuplication(
                        () => new OrderByExpression(itemType),
                        () =>
                        {
                            var bindings = new Stack<Expression>();
                            var orderByBindings = new Stack<(Expression, EnOrderingDirection)>();

                            Expression source = node;

                            while (source is MethodCallExpression methodCallExpression)
                            {
                                var methodDefinition = methodCallExpression.Method.GenericMethodDefinitionOrSelf();

                                if (methodDefinition == LinqMethods.QueryableOrderBy()
                                    || methodDefinition == LinqMethods.QueryableOrderByDescending()
                                    || methodDefinition == LinqMethods.QueryableThenBy()
                                    || methodDefinition == LinqMethods.QueryableThenByDescending())
                                {
                                    var direction = methodDefinition == LinqMethods.QueryableOrderBy() || methodDefinition == LinqMethods.QueryableThenBy()
                                        ? EnOrderingDirection.Asc
                                        : EnOrderingDirection.Desc;

                                    bindings.Push(methodCallExpression.Arguments[1]);
                                    orderByBindings.Push((methodCallExpression.Arguments[1], direction));

                                    source = methodCallExpression.Arguments[0];
                                }
                                else
                                {
                                    break;
                                }
                            }

                            if (!TryBuildJoinExpression(_context, source, bindings, itemType))
                            {
                                Visit(source);
                            }

                            foreach (var (binding, orderingDirection) in orderByBindings)
                            {
                                _context.WithinScope(new OrderByBindingExpression(orderingDirection), () => Visit(binding));
                            }
                        }));

                return node;
            }

            if (method == LinqMethods.QueryableSingle()
                || method == LinqMethods.QueryableSingleOrDefault())
            {
                _context.WithinScope(
                    new RowsFetchLimitExpression(itemType, 2),
                    () => base.VisitMethodCall(node));

                return node;
            }

            if (method == LinqMethods.QueryableFirst()
                || method == LinqMethods.QueryableFirstOrDefault())
            {
                _context.WithinScope(
                    new RowsFetchLimitExpression(itemType, 1),
                    () => base.VisitMethodCall(node));

                return node;
            }

            if (method == LinqMethods.QueryableAny())
            {
                _context.WithinScope(
                    new ProjectionExpression(itemType),
                    () =>
                    {
                        // count(*) > 0 as "Any"
                        var countAllMethodCall = new Expressions.MethodCallExpression(
                            typeof(int),
                            nameof(Queryable.Count),
                            null,
                            new[] { new SpecialExpression(itemType, "*") });

                        var binaryExpression = new Expressions.BinaryExpression(
                            typeof(bool),
                            BinaryOperator.GreaterThan,
                            countAllMethodCall,
                            new QueryParameterExpression(_context, typeof(int), static _ => Expression.Constant(0, typeof(int))));

                        _context.Apply(new NamedBindingExpression(method.Name, binaryExpression));

                        base.VisitMethodCall(node);
                    });

                return node;
            }

            if (method == LinqMethods.QueryableAll())
            {
                _context.WithinScope(
                    new ProjectionExpression(itemType),
                    () =>
                    {
                        Visit(node.Arguments[0]);

                        // count(case when <condition> then 1 else null end) = count(*) as "All"
                        _context.WithinScope(
                            new NamedBindingExpression(method.Name),
                            () =>
                            {
                                _context.WithinScope(
                                    new Expressions.BinaryExpression(typeof(bool), BinaryOperator.Equal),
                                    () =>
                                    {
                                        _context.WithinScope(
                                            new Expressions.MethodCallExpression(typeof(int), nameof(Queryable.Count), null, Array.Empty<ISqlExpression>()),
                                            () =>
                                            {
                                                _context.WithinScope(
                                                    new Expressions.ConditionalExpression(typeof(int)),
                                                    () =>
                                                    {
                                                        Visit(node.Arguments[1]);
                                                        _context.Apply(new QueryParameterExpression(_context, typeof(int), static _ => Expression.Constant(1, typeof(int))));
                                                        _context.Apply(new SpecialExpression(typeof(int?), "NULL"));
                                                    });
                                            });

                                        var countAllMethodCall = new Expressions.MethodCallExpression(
                                            typeof(int),
                                            nameof(Queryable.Count),
                                            null,
                                            new[] { new SpecialExpression(itemType, "*") });

                                        _context.Apply(countAllMethodCall);
                                    });
                            });
                    });

                return node;
            }

            if (method == LinqMethods.QueryableCount())
            {
                _context.WithinScope(
                    new ProjectionExpression(itemType),
                    () =>
                    {
                        // count(*) as "Count"
                        var countAllMethodCall = new Expressions.MethodCallExpression(
                            typeof(int),
                            nameof(Queryable.Count),
                            null,
                            new[] { new SpecialExpression(itemType, "*") });

                        _context.Apply(new NamedBindingExpression(method.Name, countAllMethodCall));

                        base.VisitMethodCall(node);
                    });

                return node;
            }

            if (method == LinqMethods.QueryableContains())
            {
                _context.WithinScope(
                    new Expressions.BinaryExpression(typeof(bool), BinaryOperator.Contains),
                    () =>
                    {
                        Visit(node.Arguments[1]);

                        if (node.Arguments[0] is ConstantExpression constantExpression
                            && constantExpression.Value is IQueryable subQuery)
                        {
                            using (Disposable.Create(constantExpression, _context.PushPath, _context.PopPath))
                            {
                                _context.Apply(TranslateSubQuery(subQuery.Expression).Expression);
                            }
                        }
                        else
                        {
                            _context.Apply(TranslateSubQuery(node.Arguments[0]).Expression);
                        }
                    });

                return node;
            }

            if (method == LinqMethods.QueryableDistinct())
            {
                Visit(node.Arguments[0]);
                var projection = _context.GetProjectionExpression(_context.Expression) ?? throw new InvalidOperationException("Unable to find distinct projection");
                projection.IsDistinct = true;

                return node;
            }

            if (method == LinqMethods.QueryableSelectMany())
            {
                throw new NotSupportedException(nameof(Queryable.SelectMany));
            }

            if (TryGetMemberInfoExpression(node.Method, out var recognized))
            {
                _context.WithinScope(recognized, () => base.VisitMethodCall(node));

                return node;
            }

            throw new NotSupportedException($"method: {node.Method}");
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var expression = TryGetMemberInfoExpression(node.Member, out var recognized)
                ? recognized
                : new SimpleBindingExpression(node.Member, node.Type);

            _context.WithinScope(expression, () => base.VisitMember(node));

            return node;
        }

        protected override Expression VisitNew(NewExpression node)
        {
            _context.Apply(new Expressions.NewExpression(node.Type));

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
                    _context.WithinScope(new NamedBindingExpression(memberInfo.Name), () => Visit(argument));
                }
            }

            return node;
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            _context.WithinScope(new Expressions.ConditionalExpression(node.Type), () => base.VisitConditional(node));

            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            _context.WithinScope(new Expressions.BinaryExpression(node.Type, node.NodeType.AsBinaryOperator()), () => base.VisitBinary(node));

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
                _context.WithinScope(new Expressions.UnaryExpression(node.Type, node.NodeType.AsUnaryOperator()), () => base.VisitUnary(node));
            }

            return node;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            _context.WithinScope(_context.GetParameterExpression(node), () => base.VisitParameter(node));

            return node;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            Visit(node.Body);

            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (typeof(IReadRepository).IsAssignableFrom(node.Type))
            {
                return base.VisitConstant(node);
            }

            _context.WithinScope(new QueryParameterExpression(_context, node.Type), () => base.VisitConstant(node));

            return node;
        }

        private void SelectAll(ISqlExpression expression)
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
                _context.Apply(column.BuildExpression(parameter));
            }

            static Expressions.ParameterExpression ExtractParameter(ISqlExpression source, Type parameterType)
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
            [NotNullWhen(true)] out ISqlExpression? expression)
        {
            expression = _memberInfoTranslators
                .Select(provider =>
                {
                    var success = provider.TryRecognize(_context, memberInfo, out var info);
                    return (success, info);
                })
                .Where(pair => pair.success)
                .Select(pair => pair.info)
                .InformativeSingleOrDefault(Amb);

            return expression != null;

            string Amb(IEnumerable<ISqlExpression?> infos)
            {
                return $"More than one expression suitable for {memberInfo.DeclaringType}.{memberInfo.Name} member";
            }
        }

        private bool TryBuildJoinExpression(
            TranslationContext context,
            Expression source,
            IReadOnlyCollection<Expression> bindings,
            Type itemType)
        {
            var type = source.Type.ExtractGenericArgumentAtOrSelf(typeof(IQueryable<>));

            var relations = bindings
               .SelectMany(binding => ExtractRelations(type, binding, _modelProvider))
               .ToHashSet();

            if (!relations.Any())
            {
                return false;
            }

            using (var recursiveEnumerable = relations.MoveNext())
            {
                context.WithoutScopeDuplication(() => new ProjectionExpression(itemType),
                    () =>
                    {
                        BuildJoinExpressionRecursive(_context, recursiveEnumerable, () => Visit(source));

                        SelectAll(_context.Parent!);
                    });
            }

            return true;

            static IReadOnlyCollection<Relation> ExtractRelations(
                Type type,
                Expression node,
                IModelProvider modelProvider)
            {
                return type.IsSubclassOfOpenGeneric(typeof(IUniqueIdentified<>))
                    ? new ExtractRelationsExpressionVisitor(modelProvider).Extract(node)
                    : Array.Empty<Relation>();
            }

            static void BuildJoinExpressionRecursive(
                TranslationContext context,
                IRecursiveEnumerable<Relation> recursiveEnumerable,
                Action? action)
            {
                if (recursiveEnumerable.TryMoveNext(out var relation))
                {
                    context.WithinScope(
                        new JoinExpression(),
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

        private SqlExpression TranslateSubQuery(Expression expression)
        {
            var context = _context.Clone();
            var visitor = new TranslationExpressionVisitor(context, _modelProvider, _preprocessor, _memberInfoTranslators);
            return visitor.Translate(expression);
        }
    }
}