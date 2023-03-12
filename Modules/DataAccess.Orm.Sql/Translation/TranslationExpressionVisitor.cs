namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Basics;
    using Basics.Enumerations;
    using Basics.Primitives;
    using Expressions;
    using Linq;
    using Model;
    using Transaction;
    using BinaryExpression = System.Linq.Expressions.BinaryExpression;
    using ConditionalExpression = System.Linq.Expressions.ConditionalExpression;
    using MethodCallExpression = System.Linq.Expressions.MethodCallExpression;
    using NewExpression = System.Linq.Expressions.NewExpression;
    using ParameterExpression = System.Linq.Expressions.ParameterExpression;
    using UnaryExpression = System.Linq.Expressions.UnaryExpression;

    [SuppressMessage("Analysis", "CA1502", Justification = "complex expression visitor")]
    [SuppressMessage("Analysis", "CA1506", Justification = "complex expression visitor")]
    internal class TranslationExpressionVisitor : ExpressionVisitor
    {
        private readonly TranslationContext _context;
        private readonly IModelProvider _modelProvider;
        private readonly ILinqExpressionPreprocessorComposite _preprocessor;
        private readonly IEnumerable<IUnknownExpressionTranslator> _unknownExpressionTranslators;

        private TranslationExpressionVisitor(
            TranslationContext context,
            IModelProvider modelProvider,
            ILinqExpressionPreprocessorComposite preprocessor,
            IEnumerable<IUnknownExpressionTranslator> unknownExpressionTranslators)
        {
            _context = context;
            _modelProvider = modelProvider;
            _preprocessor = preprocessor;
            _unknownExpressionTranslators = unknownExpressionTranslators;
        }

        internal static MethodInfo GetInsertValuesMethod { get; } = new MethodFinder(
                typeof(TranslationExpressionVisitor),
                nameof(GetInsertValues),
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod)
            {
                ArgumentTypes = new[] { typeof(IModelProvider), typeof(IReadOnlyCollection<IDatabaseEntity>) }
            }
            .FindMethod() ?? throw new InvalidOperationException("SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.TranslationExpressionVisitor.GetInsertValues()");

        public static SqlExpression Translate(
            TranslationContext context,
            IModelProvider modelProvider,
            ILinqExpressionPreprocessorComposite preprocessor,
            IEnumerable<IUnknownExpressionTranslator> unknownExpressionTranslators,
            Expression expression)
        {
            var visitor = new TranslationExpressionVisitor(
                context,
                modelProvider,
                preprocessor,
                unknownExpressionTranslators);

            _ = visitor.Visit(preprocessor.Visit(expression));

            return new SqlExpression(
                visitor._context.SqlExpression ?? throw new InvalidOperationException("Sql expression wasn't built"),
                visitor._context.BuildCommandParametersExtractor(preprocessor));
        }

        public sealed override Expression Visit(Expression node)
        {
            using (_context.WithinPathScope(node))
            {
                _ = base.Visit(node);

                return node;
            }
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var method = node.Method.GenericMethodDefinitionOrSelf();

            var itemType = node.Type.ExtractQueryableItemType();

            if (method == LinqMethods.CachedExpression()
                || method == LinqMethods.CachedInsertExpression()
                || method == LinqMethods.CachedUpdateExpression()
                || method == LinqMethods.CachedDeleteExpression()
                || method == LinqMethods.WithDependencyContainer())
            {
                _ = Visit(node.Arguments[0]);

                return node;
            }

            if (method == LinqMethods.RepositoryInsert())
            {
                _context.WithinScope(
                    new BatchExpression(),
                    () =>
                    {
                        var getInsertValuesMethodCallExpression = Expression.Call(
                            null,
                            GetInsertValuesMethod,
                            Expression.Constant(_modelProvider),
                            Expression.Constant(new List<IDatabaseEntity>(), typeof(IReadOnlyCollection<IDatabaseEntity>)));

                        using (_context.WithinPathScope(node.Arguments[1]))
                        using (_context.WithinPathScope(getInsertValuesMethodCallExpression))
                        {
                            _ = (IAdvancedDatabaseTransaction)((ConstantExpression)node.Arguments[0]).Value;
                            var entities = (IReadOnlyCollection<IDatabaseEntity>)((ConstantExpression)node.Arguments[1]).Value;
                            var insertBehavior = (EnInsertBehavior)((ConstantExpression)node.Arguments[2]).Value;

                            var map = entities
                                .SelectMany(_modelProvider.Flatten)
                                .Distinct(new UniqueIdentifiedEqualityComparer())
                                .ToDictionary(entity => new EntityKey(entity), entity => entity);

                            var stacks = map
                                .Values
                                .OrderByDependencies(entity => new EntityKey(entity), GetDependencies(_modelProvider, map))
                                .Stack(entity => entity.GetType());

                            foreach (var (type, stack) in stacks)
                            {
                                var table = _modelProvider.Tables[type];

                                _context.WithinScope(
                                    new InsertExpression(type, insertBehavior),
                                    () =>
                                    {
                                        foreach (var entity in stack)
                                        {
                                            _context.WithinScope(
                                                new ValuesExpression(),
                                                () =>
                                                {
                                                    foreach (var column in table.Columns.Values.Where(column => !column.IsMultipleRelation))
                                                    {
                                                        _context.Apply(new QueryParameterExpression(_context, column.Type));
                                                    }
                                                });
                                        }
                                    });
                            }
                        }
                    });

                return node;
            }

            if (method == LinqMethods.RepositoryUpdate())
            {
                _context.WithinScope(
                    new UpdateExpression(itemType),
                    () => base.VisitMethodCall(node));

                return node;
            }

            if (method == LinqMethods.RepositoryUpdateSet()
                || method == LinqMethods.RepositoryChainedUpdateSet())
            {
                _context.WithoutScopeDuplication(
                    () => new SetExpression(),
                    () => base.VisitMethodCall(node));

                return node;
            }

            if (method == LinqMethods.RepositoryDelete())
            {
                _context.WithinScope(
                    new DeleteExpression(itemType),
                    () =>
                    {
                        base.VisitMethodCall(node);
                        _context.ReverseLambdaParametersNames();
                    });

                return node;
            }

            if (method == LinqMethods.RepositoryAll())
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
                            var expressions = new[] { node.Arguments[1] };

                            if (TryBuildJoinExpression(_context, node.Arguments[0], expressions, itemType))
                            {
                                _ = Visit(expressions[0]);
                            }
                            else
                            {
                                base.VisitMethodCall(node);
                            }
                        }));

                return node;
            }

            if (method == LinqMethods.QueryableWhere()
                || method == LinqMethods.RepositoryUpdateWhere()
                || method == LinqMethods.RepositoryDeleteWhere())
            {
                _context.WithinConditionalScope(
                    parent => parent is ProjectionExpression || parent is JoinExpression,
                    action => _context.WithoutScopeDuplication(
                        () => new NamedSourceExpression(itemType, _context),
                        action),
                    () => _context.WithoutScopeDuplication(
                        () => new FilterExpression(),
                        () =>
                        {
                            var expressions = new[] { node.Arguments[1] };

                            if (TryBuildJoinExpression(_context, node.Arguments[0], expressions, itemType))
                            {
                                _ = Visit(expressions[0]);
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
                            var expressions = new Stack<Expression>();
                            var orderByExpressions = new Stack<(Expression, EnOrderingDirection)>();

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

                                    expressions.Push(methodCallExpression.Arguments[1]);
                                    orderByExpressions.Push((methodCallExpression.Arguments[1], direction));

                                    source = methodCallExpression.Arguments[0];
                                }
                                else
                                {
                                    break;
                                }
                            }

                            if (!TryBuildJoinExpression(_context, source, expressions, itemType))
                            {
                                _ = Visit(source);
                            }

                            foreach (var (expression, orderingDirection) in orderByExpressions)
                            {
                                _context.WithinScope(new OrderByExpressionExpression(orderingDirection), () => Visit(expression));
                            }
                        }));

                return node;
            }

            if (method == LinqMethods.Explain())
            {
                var analyze = (bool)((ConstantExpression)node.Arguments[1]).Value;

                _context.WithinScope(
                    new ExplainExpression(analyze),
                    () => Visit(node.Arguments[0]));

                return node;
            }

            if (method == LinqMethods.QueryableSingle()
                || method == LinqMethods.QueryableSingleOrDefault())
            {
                _context.WithinScope(
                    new RowsFetchLimitExpression(2),
                    () => base.VisitMethodCall(node));

                return node;
            }

            if (method == LinqMethods.QueryableFirst()
                || method == LinqMethods.QueryableFirstOrDefault())
            {
                _context.WithinScope(
                    new RowsFetchLimitExpression(1),
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
                            new[] { new SpecialExpression("*") });

                        var binaryExpression = new Expressions.BinaryExpression(
                            typeof(bool),
                            BinaryOperator.GreaterThan,
                            countAllMethodCall,
                            new QueryParameterExpression(
                                _context,
                                typeof(int),
                                static (_, _) => Expression.Constant(0, typeof(int))));

                        _context.Apply(new RenameExpression(typeof(bool), method.Name, binaryExpression));

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
                        _ = Visit(node.Arguments[0]);

                        // count(case when <condition> then 1 else null end) = count(*) as "All"
                        _context.WithinScope(
                            new RenameExpression(typeof(bool), method.Name),
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
                                                        _ = Visit(node.Arguments[1]);
                                                        _context.Apply(new QueryParameterExpression(_context, typeof(int), static (_, _) => Expression.Constant(1, typeof(int))));
                                                        _context.Apply(new SpecialExpression("NULL"));
                                                    });
                                            });

                                        _context.Apply(new Expressions.MethodCallExpression(
                                            typeof(int),
                                            nameof(Queryable.Count),
                                            null,
                                            new[] { new SpecialExpression("*") }));
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
                            new[] { new SpecialExpression("*") });

                        _context.Apply(new RenameExpression(typeof(int), method.Name, countAllMethodCall));

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
                        _ = Visit(node.Arguments[1]);

                        if (node.Arguments[0] is ConstantExpression constantExpression
                            && constantExpression.Value is IQueryable subQuery)
                        {
                            using (_context.WithinPathScope(constantExpression))
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
                _ = Visit(node.Arguments[0]);

                var projection = ExtractProjectionExpression(_context.SqlExpression)
                                 ?? throw new InvalidOperationException("Unable to find distinct projection");

                projection.IsDistinct = true;

                return node;
            }

            if (method == LinqMethods.QueryableCast())
            {
                _ = Visit(node.Arguments[0]);

                return node;
            }

            if (TryTranslateUnknownExpression(node))
            {
                return node;
            }

            throw new NotSupportedException($"method: {node.Method}");
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (TryTranslateUnknownExpression(node))
            {
                return node;
            }

            _context.WithinScope(
                new ColumnExpression(node.Member, node.Type),
                () => base.VisitMember(node));

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
                    _ = Visit(argument);
                }
                else
                {
                    _context.WithinScope(
                        new RenameExpression(argument.Type, memberInfo.Name),
                        () => Visit(argument));
                }
            }

            return node;
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            _context.WithinScope(
                new Expressions.ConditionalExpression(node.Type),
                () => base.VisitConditional(node));

            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            _context.WithinScope(
                new Expressions.BinaryExpression(node.Type, node.NodeType.AsBinaryOperator()),
                () => base.VisitBinary(node));

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
                _context.WithinScope(
                    new Expressions.UnaryExpression(node.Type, node.NodeType.AsUnaryOperator()),
                    () => base.VisitUnary(node));
            }

            return node;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (_context.Expression != null
                && (ExtractUpdateQueryRootExpressionVisitor.IsUpdateQuery(_context.Expression)
                    || ExtractDeleteQueryRootExpressionVisitor.IsDeleteQuery(_context.Expression)))
            {
                return node;
            }

            _context.WithinScope(
                ExtractParametersVisitor.TryExtractParameter(_context.Outer!, node.Type, out var outerParameterExpression)
                    ? outerParameterExpression
                    : new Expressions.ParameterExpression(_context, node.Type),
                () => base.VisitParameter(node));

            return node;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            _ = Visit(node.Body);

            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (typeof(IRepository).IsAssignableFrom(node.Type)
                || typeof(IDatabaseContext).IsAssignableFrom(node.Type))
            {
                return base.VisitConstant(node);
            }

            _context.WithinScope(
                new QueryParameterExpression(_context, node.Type),
                () => base.VisitConstant(node));

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

            if (!ExtractParametersVisitor.TryExtractParameter(projection.Source, projection.Type, out var parameter))
            {
                return;
            }

            var flatProjectionColumns = _modelProvider
                .Columns(projection.Type)
                .Where(column => !column.IsMultipleRelation)
                .OrderBy(column => column.Name);

            foreach (var column in flatProjectionColumns)
            {
                _context.Apply(column.BuildExpression(parameter));
            }
        }

        private bool TryTranslateUnknownExpression(Expression expression)
        {
            return _unknownExpressionTranslators
                .Where(translator => translator.TryTranslate(_context, expression, this))
                .InformativeSingleOrDefault(Amb, expression) != null;

            static string Amb(Expression expression, IEnumerable<IUnknownExpressionTranslator> infos)
            {
                return $"More than one translations suitable for expression: {expression}";
            }
        }

        private static ProjectionExpression? ExtractProjectionExpression(ISqlExpression? expression)
        {
            switch (expression)
            {
                case NamedSourceExpression namedSourceExpression:
                    return ExtractProjectionExpression(namedSourceExpression.Source);
                case FilterExpression filterExpression:
                    return ExtractProjectionExpression(filterExpression.Source);
                case ProjectionExpression projectionExpression:
                    return projectionExpression;
                case JoinExpression:
                    throw new InvalidOperationException("Ambiguous reference to join expression source");
                case OrderByExpression orderByExpression:
                    return ExtractProjectionExpression(orderByExpression.Source);
                default:
                    return default;
            }
        }

        private bool TryBuildJoinExpression(
            TranslationContext context,
            Expression source,
            IReadOnlyCollection<Expression> expressions,
            Type itemType)
        {
            var type = source.Type.ExtractQueryableItemType();

            var relations = expressions
               .SelectMany(expression => ExtractRelations(type, expression, _modelProvider))
               .ToHashSet();

            if (!relations.Any())
            {
                return false;
            }

            using (var recursiveEnumerable = relations.MoveNext())
            {
                context.WithoutScopeDuplication(
                    () => new ProjectionExpression(itemType),
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
                    ? ExtractRelationsExpressionVisitor.Extract(node, modelProvider)
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
                            context.WithoutScopeDuplication(
                                () => new NamedSourceExpression(relation.Target, context),
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
                        new ColumnExpression(
                            relation.Target.GetProperty(nameof(IUniqueIdentified.PrimaryKey)),
                            typeof(Guid),
                            ExtractParametersVisitor.ExtractParameter(context.Outer!, relation.Target)),
                        new ColumnExpression(
                            relation.Target.GetProperty(nameof(IUniqueIdentified.PrimaryKey)),
                            typeof(Guid),
                            new ColumnExpression(
                                relation.Property.Reflected,
                                relation.Target,
                                ExtractParametersVisitor.ExtractParameter(context.Outer!, relation.Source)))));
                }
            }
        }

        private static Func<IUniqueIdentified, IEnumerable<IUniqueIdentified>> GetDependencies(
            IModelProvider modelProvider,
            IReadOnlyDictionary<EntityKey, IUniqueIdentified> map)
        {
            return entity =>
            {
                var table = modelProvider.Tables[entity.GetType()];

                return table
                    .Columns
                    .Values
                    .Where(column => column.IsRelation)
                    .Select(DependencySelector(table, entity, map))
                    .Where(dependency => dependency != null)
                    .Select(dependency => dependency!);

                static Func<ColumnInfo, IUniqueIdentified?> DependencySelector(
                    ITableInfo table,
                    IUniqueIdentified entity,
                    IReadOnlyDictionary<EntityKey, IUniqueIdentified> map)
                {
                    return column => table.IsMtmTable
                        ? map[new EntityKey(column.Relation.Target, column.GetValue(entity) !)]
                        : column.GetRelationValue(entity);
                }
            };
        }

        private static IReadOnlyDictionary<string, ConstantExpression> GetInsertValues(
                IModelProvider modelProvider,
                IReadOnlyCollection<IDatabaseEntity> entities)
        {
            var map = entities
                .SelectMany(modelProvider.Flatten)
                .Distinct(new UniqueIdentifiedEqualityComparer())
                .ToDictionary(entity => new EntityKey(entity), entity => entity);

            return map
                .Values
                .OrderByDependencies(entity => new EntityKey(entity), GetDependencies(modelProvider, map))
                .SelectMany(entity => modelProvider
                    .Tables[entity.GetType()]
                    .Columns
                    .Values
                    .Where(column => !column.IsMultipleRelation)
                    .Select(column =>
                    {
                        var value = column.GetValue(entity);

                        return column.IsJsonColumn
                            ? Expression.Constant(new DatabaseJsonObject(value, column.Type), typeof(DatabaseJsonObject))
                            : Expression.Constant(value, column.Type);
                    }))
                .Select((expression, index) => (name: TranslationContext.CommandParameterFormat.Format(index), expression))
                .ToDictionary(
                    pair => pair.name,
                    pair => pair.expression,
                    StringComparer.OrdinalIgnoreCase);
        }

        private SqlExpression TranslateSubQuery(Expression expression)
        {
            return Translate(
                _context.Clone(),
                _modelProvider,
                _preprocessor,
                _unknownExpressionTranslators,
                expression);
        }
    }
}