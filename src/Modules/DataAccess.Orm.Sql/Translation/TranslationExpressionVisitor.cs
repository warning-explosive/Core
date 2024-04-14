namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Basics;
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

    [SuppressMessage("Analysis", "CA1502", Justification = "complex infrastructural code")]
    [SuppressMessage("Analysis", "CA1506", Justification = "complex infrastructural code")]
    internal class TranslationExpressionVisitor : ExpressionVisitor
    {
        private readonly Expression _expression;
        private readonly TranslationContext _context;
        private readonly IModelProvider _modelProvider;
        private readonly ILinqExpressionPreprocessorComposite _preprocessor;
        private readonly IEnumerable<IUnknownExpressionTranslator> _unknownExpressionTranslators;

        private TranslationExpressionVisitor(
            Expression expression,
            TranslationContext context,
            IModelProvider modelProvider,
            ILinqExpressionPreprocessorComposite preprocessor,
            IEnumerable<IUnknownExpressionTranslator> unknownExpressionTranslators)
        {
            _expression = expression;
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
                expression,
                context,
                modelProvider,
                preprocessor,
                unknownExpressionTranslators);

            visitor.Visit(preprocessor.Visit(expression));

            return new SqlExpression(
                visitor._context.SqlExpression ?? throw new InvalidOperationException("Sql expression wasn't built"),
                visitor._context.BuildCommandParametersExtractor(preprocessor));
        }

        public sealed override Expression Visit(Expression node)
        {
            using (_context.WithinPathScope(node))
            {
                base.Visit(node);

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
                Visit(node.Arguments[0]);

                return node;
            }

            if (method == LinqMethods.RepositoryInsert())
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

                    var expressions = stacks
                        .Select(pair =>
                        {
                            var (type, stack) = pair;
                            var table = _modelProvider.Tables[type];

                            var values = stack
                                .Select(_ =>
                                {
                                    var values = table
                                        .Columns
                                        .Values
                                        .Where(column => !column.IsMultipleRelation)
                                        .Select(column =>
                                        {
                                            var name = _context.NextCommandParameterName();
                                            _context.CaptureCommandParameterExtractor(name, null);
                                            return new QueryParameterExpression(column.Type, name);
                                        })
                                        .ToList();

                                    return new ValuesExpression(values);
                                })
                                .ToList();

                            return new InsertExpression(type, insertBehavior, values);
                        })
                        .ToList();

                    var batchExpression = new BatchExpression(expressions);
                    _context.Remember(batchExpression);
                }

                return node;
            }

            if (method == LinqMethods.RepositoryUpdate())
            {
                var updateExpression = new UpdateExpression(itemType);
                _context.Remember(updateExpression);

                return node;
            }

            if (method == LinqMethods.RepositoryUpdateSet()
                || method == LinqMethods.RepositoryChainedUpdateSet())
            {
                Visit(node.Arguments[0]);
                var source = (UpdateExpression)_context.SqlExpression;
                Visit(node.Arguments[1]);
                var assignments = (Expressions.BinaryExpression)_context.SqlExpression;
                var setExpression = new SetExpression(source, new[] { assignments });
                _context.Remember(setExpression);

                return node;
            }

            if (method == LinqMethods.RepositoryDelete())
            {
                var deleteExpression = new DeleteExpression(itemType);
                _context.Remember(deleteExpression);

                return node;
            }

            if (method == LinqMethods.RepositoryAll())
            {
                if (_context.IsOuterExpression() && !itemType.IsSqlView())
                {
                    var querySourceExpression = new QuerySourceExpression(itemType);
                    var parameterExpression = new Expressions.ParameterExpression(itemType, _context.NextLambdaParameterName());
                    var namedSourceExpression = new NamedSourceExpression(itemType, querySourceExpression, parameterExpression);
                    var expressions = SelectAll(itemType, parameterExpression);
                    var projectionExpression = new ProjectionExpression(itemType, namedSourceExpression, expressions);

                    _context.Remember(projectionExpression);
                }
                else
                {
                    var querySourceExpression = new QuerySourceExpression(itemType);

                    _context.Remember(querySourceExpression);
                }

                return node;
            }

            if (method == LinqMethods.QueryableSelect())
            {
                // TODO: join expression
                Visit(node.Arguments[0]);
                var sourceItemType = GetSourceItemType(_context.SqlExpression);
                var parameterExpression = new Expressions.ParameterExpression(sourceItemType, _context.NextLambdaParameterName());
                var unnamedSource = _context.SqlExpression is QuerySourceExpression
                    ? _context.SqlExpression
                    : new ParenthesesExpression(_context.SqlExpression);
                var source = new NamedSourceExpression(sourceItemType, unnamedSource, parameterExpression);

                IReadOnlyCollection<ISqlExpression> expressions;

                using (_context.OpenParameterScope(source.Parameter))
                {
                    Visit(node.Arguments[1]);
                    expressions = _context.SqlExpression is Expressions.NewExpression newExpression
                        ? newExpression.Parameters
                        : new[] { _context.SqlExpression };
                }

                var projectionExpression = new ProjectionExpression(itemType, source, expressions);
                _context.Remember(projectionExpression);

                return node;
            }

            if (method == LinqMethods.QueryableWhere()
                || method == LinqMethods.RepositoryUpdateWhere()
                || method == LinqMethods.RepositoryDeleteWhere())
            {
                // TODO: join expression
                Visit(node.Arguments[0]);
                var source = _context.SqlExpression;

                var sourceItemType = GetSourceItemType(source);
                var sourceSource = GetSource(source);
                var sourceSourceItemType = sourceSource != null
                    ? GetSourceItemType(sourceSource)
                    : null;

                switch (source)
                {
                    case ProjectionExpression sourceProjectionExpression when sourceSourceItemType != null && sourceItemType != sourceSourceItemType:
                    {
                        /*
                         * select made a projection
                         */

                        var parameterExpression = new Expressions.ParameterExpression(itemType, _context.NextLambdaParameterName());

                        ISqlExpression predicate;

                        using (_context.OpenParameterScope(parameterExpression))
                        {
                            Visit(node.Arguments[1]);
                            predicate = _context.SqlExpression;
                        }

                        /*
                           TODO: replace star with anonymous projection
                           var method = node.Method.GenericMethodDefinitionOrSelf();

                           if (method == LinqMethods.QueryableSelect())
                           {
                               var itemType = node.Type.ExtractQueryableItemType();
                               var sourceItemType = node.Arguments[0].Type.ExtractQueryableItemType();

                               if (itemType != sourceItemType
                                   && itemType.IsPrimitive())
                               {
                                   _ = (itemType, sourceItemType);

                                   var propertyName = node.Arguments[1].UnwrapUnaryExpression() is LambdaExpression { Body: MemberExpression memberExpression }
                                       ? memberExpression.Member.Name
                                       : throw new NotSupportedException(node.Arguments[1].GetType().FullName);
                                   var dynamicClass = new DynamicClass("qwe_asm", $"{sourceItemType.Name}_To_{itemType.Name}_By_{propertyName}")
                                       .HasProperties(new DynamicProperty(itemType, propertyName));
                                   var type = _dynamicClassProvider.CreateType(dynamicClass);
                                   var ctor = type.GetConstructor(Array.Empty<Type>());
                                   var arguments = new Expression[] { memberExpression };
                                   var property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);

                                   var newExpression = Expression.New(ctor);
                                   var memberInitExpression = Expression.MemberInit(newExpression, Expression.Bind(property, memberExpression));
                                   _ = (ctor, arguments, property);
                               }
                           }
                         */

                        source = new NamedSourceExpression(itemType, new ParenthesesExpression(sourceProjectionExpression), parameterExpression);
                        IReadOnlyCollection<ISqlExpression> expressions = itemType.IsPrimitive()
                            ? new ISqlExpression[] { new StarExpression() }
                            : SelectAll(itemType, parameterExpression);
                        var projectionExpression = new ProjectionExpression(itemType, source, expressions);
                        var filterExpression = new FilterExpression(itemType, projectionExpression, predicate);
                        _context.Remember(filterExpression);

                        break;
                    }

                    case ProjectionExpression sourceProjectionExpression:
                    {
                        /*
                         * attach predicate to a projection selection
                         */

                        var parameterExpression = sourceProjectionExpression.Source is NamedSourceExpression namedSourceExpression
                            ? namedSourceExpression.Parameter
                            : new Expressions.ParameterExpression(itemType, _context.NextLambdaParameterName());

                        ISqlExpression predicate;

                        using (_context.OpenParameterScope(parameterExpression))
                        {
                            Visit(node.Arguments[1]);
                            predicate = _context.SqlExpression;
                        }

                        var filterExpression = new FilterExpression(itemType, sourceProjectionExpression, predicate);
                        _context.Remember(filterExpression);

                        break;
                    }

                    case QuerySourceExpression querySourceExpression:
                    {
                        /*
                         * filter raw table or view without projections
                         */

                        var parameterExpression = new Expressions.ParameterExpression(itemType, _context.NextLambdaParameterName());

                        ISqlExpression predicate;

                        using (_context.OpenParameterScope(parameterExpression))
                        {
                            Visit(node.Arguments[1]);
                            predicate = _context.SqlExpression;
                        }

                        var namedSourceExpression = new NamedSourceExpression(itemType, querySourceExpression, parameterExpression);
                        var expressions = SelectAll(itemType, parameterExpression);
                        var projectionExpression = new ProjectionExpression(itemType, namedSourceExpression, expressions);
                        var filterExpression = new FilterExpression(itemType, projectionExpression, predicate);
                        _context.Remember(filterExpression);

                        break;
                    }

                    default:
                    {
                        throw new NotSupportedException(source.GetType().FullName);
                    }
                }

                return node;
            }

            if (method == LinqMethods.QueryableOrderBy()
                || method == LinqMethods.QueryableOrderByDescending()
                || method == LinqMethods.QueryableThenBy()
                || method == LinqMethods.QueryableThenByDescending())
            {
                // TODO:
                /*_context.WithinConditionalScope(
                    outer => outer is ProjectionExpression || outer is JoinExpression,
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
                                Visit(source);
                            }

                            foreach (var (expression, orderingDirection) in orderByExpressions)
                            {
                                _context.WithinScope(new OrderByExpressionExpression(orderingDirection), () => Visit(expression));
                            }
                        }));

                return node;*/
            }

            if (method == LinqMethods.Explain())
            {
                Visit(node.Arguments[0]);
                var source = _context.SqlExpression;
                var analyze = (bool)((ConstantExpression)node.Arguments[1]).Value;
                var explainExpression = new ExplainExpression(source, analyze);
                _context.Remember(explainExpression);

                return node;
            }

            if (method == LinqMethods.QueryableSingle()
                || method == LinqMethods.QueryableSingleOrDefault())
            {
                Visit(node.Arguments[0]);
                var source = _context.SqlExpression;
                var rowsFetchLimitExpression = new RowsFetchLimitExpression(source, 2);
                _context.Remember(rowsFetchLimitExpression);

                return node;
            }

            if (method == LinqMethods.QueryableFirst()
                || method == LinqMethods.QueryableFirstOrDefault())
            {
                Visit(node.Arguments[0]);
                var source = _context.SqlExpression;
                var rowsFetchLimitExpression = new RowsFetchLimitExpression(source, 1);
                _context.Remember(rowsFetchLimitExpression);

                return node;
            }

            if (method == LinqMethods.QueryableAny())
            {
                /*
                 * count(*) > 0 as "Any"
                 */

                Visit(node.Arguments[0]);
                var parameterExpression = new Expressions.ParameterExpression(itemType, _context.NextLambdaParameterName());
                ISqlExpression source = new NamedSourceExpression(itemType, new ParenthesesExpression(_context.SqlExpression), parameterExpression);
                var left = new Expressions.MethodCallExpression(typeof(int), nameof(Queryable.Count), null, new[] { new StarExpression() });
                var name = _context.NextCommandParameterName();
                var extractor = new Func<CommandParameterExtractionContext, ConstantExpression>(static _ => Expression.Constant(0, typeof(int)));
                _context.CaptureCommandParameterExtractor(name, extractor);
                var right = new QueryParameterExpression(typeof(int), name);
                var binaryExpression = new Expressions.BinaryExpression(typeof(bool), BinaryOperator.GreaterThan, left, right);
                var parenthesesExpression = new ParenthesesExpression(binaryExpression);
                var renameExpression = new RenameExpression(typeof(bool), method.Name, parenthesesExpression);
                var projectionExpression = new ProjectionExpression(itemType, source, new[] { renameExpression });
                _context.Remember(projectionExpression);

                return node;
            }

            if (method == LinqMethods.QueryableAll())
            {
                /*
                 * (count(case when <condition> then 1 else null end) = count(*)) as "All"
                 */

                Visit(node.Arguments[0]);
                var parameterExpression = new Expressions.ParameterExpression(itemType, _context.NextLambdaParameterName());
                var source = new NamedSourceExpression(itemType, _context.SqlExpression, parameterExpression);

                using (_context.OpenParameterScope(parameterExpression))
                {
                    Visit(node.Arguments[1]);
                }

                var when = _context.SqlExpression;
                var name = _context.NextCommandParameterName();
                var extractor = new Func<CommandParameterExtractionContext, ConstantExpression>(static _ => Expression.Constant(1, typeof(int)));
                _context.CaptureCommandParameterExtractor(name, extractor);
                var then = new QueryParameterExpression(typeof(int), name);
                var @else = new NullExpression();
                var conditionalExpression = new Expressions.ConditionalExpression(typeof(int), when, then, @else);
                var left = new Expressions.MethodCallExpression(typeof(int), nameof(Queryable.Count), null, new[] { conditionalExpression });
                var right = new Expressions.MethodCallExpression(typeof(int), nameof(Queryable.Count), null, new[] { new StarExpression() });
                var binaryExpression = new Expressions.BinaryExpression(typeof(bool), BinaryOperator.Equal, left, right);
                var parenthesesExpression = new ParenthesesExpression(binaryExpression);
                var renameExpression = new RenameExpression(typeof(bool), method.Name, parenthesesExpression);
                var projectionExpression = new ProjectionExpression(itemType, source, new[] { renameExpression });
                _context.Remember(projectionExpression);

                return node;
            }

            if (method == LinqMethods.QueryableCount())
            {
                /*
                 * count(*) as "Count"
                 */

                Visit(node.Arguments[0]);
                var parameterExpression = new Expressions.ParameterExpression(itemType, _context.NextLambdaParameterName());
                var source = new NamedSourceExpression(itemType, new ParenthesesExpression(_context.SqlExpression), parameterExpression);
                var countAllMethodCall = new Expressions.MethodCallExpression(typeof(int), nameof(Queryable.Count), null, new[] { new StarExpression() });
                var renameExpression = new RenameExpression(typeof(int), method.Name, countAllMethodCall);
                var projectionExpression = new ProjectionExpression(itemType, source, new[] { renameExpression });
                _context.Remember(projectionExpression);

                return node;
            }

            if (method == LinqMethods.QueryableContains())
            {
                if (node.Arguments[0] is not ConstantExpression constantExpression
                    || constantExpression.Value is not IQueryable subQuery)
                {
                    throw new InvalidOperationException("Unable to translate sub-query");
                }

                Visit(node.Arguments[1]);
                var left = _context.SqlExpression;
                ISqlExpression right;

                using (_context.WithinPathScope(constantExpression))
                {
                    right = TranslateSubQuery(subQuery.Expression).Expression;
                }

                var binaryExpression = new Expressions.BinaryExpression(typeof(bool), BinaryOperator.Contains, left, right);
                _context.Remember(binaryExpression);

                return node;

                SqlExpression TranslateSubQuery(Expression expression)
                {
                    return Translate(
                        _context.Clone(),
                        _modelProvider,
                        _preprocessor,
                        _unknownExpressionTranslators,
                        expression);
                }
            }

            if (method == LinqMethods.QueryableDistinct())
            {
                Visit(node.Arguments[0]);
                var projection = (ProjectionExpression)_context.SqlExpression;
                projection.IsDistinct = true;

                return node;
            }

            if (method == LinqMethods.QueryableCast())
            {
                Visit(node.Arguments[0]);

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

            Visit(node.Expression);
            var source = _context.SqlExpression;
            var expression = new ColumnExpression(node.Member, node.Type, source);
            _context.Remember(expression);

            return node;
        }

        protected override Expression VisitNew(NewExpression node)
        {
            var parameters = new List<ISqlExpression>(node.Arguments.Count);

            foreach (var (memberInfo, argument) in node.Members.Zip(node.Arguments, (memberInfo, argument) => (memberInfo, argument)))
            {
                if (argument is MemberExpression memberExpression
                    && memberExpression.Member.MemberType == MemberTypes.Property
                    && memberExpression.Member.Name.Equals(memberInfo.Name, StringComparison.OrdinalIgnoreCase))
                {
                    Visit(argument);
                    var expression = _context.SqlExpression;
                    parameters.Add(expression);
                }
                else
                {
                    Visit(argument);
                    var expression = _context.SqlExpression is ColumnExpression
                        ? _context.SqlExpression
                        : new ParenthesesExpression(_context.SqlExpression);
                    var renameExpression = new RenameExpression(argument.Type, memberInfo.Name, expression);
                    parameters.Add(renameExpression);
                }
            }

            var newExpression = new Expressions.NewExpression(node.Type, parameters);
            _context.Remember(newExpression);

            return node;
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            Visit(node.Test);
            var when = _context.SqlExpression;
            Visit(node.IfTrue);
            var then = _context.SqlExpression;
            Visit(node.IfFalse);
            var @else = _context.SqlExpression;
            var conditionalExpression = new Expressions.ConditionalExpression(node.Type, when, then, @else);
            _context.Remember(conditionalExpression);

            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            Visit(node.Left);
            var left = _context.SqlExpression;
            Visit(node.Right);
            var right = _context.SqlExpression;
            var binaryExpression = new Expressions.BinaryExpression(node.Type, node.NodeType.AsBinaryOperator(), left, right);
            _context.Remember(binaryExpression);

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
                Visit(node.Operand);
                var source = _context.SqlExpression;
                var unaryExpression = new Expressions.UnaryExpression(node.Type, node.NodeType.AsUnaryOperator(), source);
                _context.Remember(unaryExpression);
            }

            return node;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (ExtractUpdateQueryRootExpressionVisitor.IsUpdateQuery(_expression)
                || ExtractDeleteQueryRootExpressionVisitor.IsDeleteQuery(_expression))
            {
                return node;
            }

            var parameterExpression = _context.ParameterExpression ?? new Expressions.ParameterExpression(node.Type, _context.NextLambdaParameterName());
            _context.Remember(parameterExpression);

            // todo;
            /*ExtractParametersVisitor.TryExtractParameter(_context.Outer, node.Type, out var outerParameterExpression)
                ? outerParameterExpression
                : new Expressions.ParameterExpression(_context, node.Type),*/

            return node;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            Visit(node.Body);

            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            var name = _context.NextCommandParameterName();
            _context.CaptureCommandParameterExtractor(name, null);
            var queryParameterExpression = new QueryParameterExpression(node.Type, name);
            _context.Remember(queryParameterExpression);

            return node;
        }

        private static ISqlExpression? GetSource(ISqlExpression source)
        {
            return source switch
            {
                FilterExpression filterExpression => GetSource(filterExpression.Source),
                NamedSourceExpression namedSourceExpression => namedSourceExpression.Source,
                ProjectionExpression projectionExpression => GetSource(projectionExpression.Source),
                QuerySourceExpression => default,
                _ => throw new NotSupportedException(source.GetType().FullName)
            };
        }

        private static Type GetSourceItemType(ISqlExpression source)
        {
            return source switch
            {
                FilterExpression filterExpression => GetSourceItemType(filterExpression.Source),
                ProjectionExpression projectionExpression => projectionExpression.ItemType,
                QuerySourceExpression querySourceExpression => querySourceExpression.ItemType,
                _ => throw new NotSupportedException(source.GetType().FullName)
            };
        }

        private IReadOnlyCollection<ColumnExpression> SelectAll(
            Type type,
            Expressions.ParameterExpression parameterExpression)
        {
            if (!type.IsClass
                || type.IsPrimitive()
                || type.IsCollection())
            {
                throw new InvalidOperationException(nameof(SelectAll));
            }

            var columns = _modelProvider
                .Columns(type)
                .Values
                .Where(column => !column.IsMultipleRelation);

            return columns
                .Select(column => column.BuildExpression(parameterExpression))
                .ToList();
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

        // TODO:
        /*private bool TryBuildJoinExpression(
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
                    projection =>
                    {
                        BuildJoinExpressionRecursive(_context, _modelProvider, recursiveEnumerable, () => Visit(source));

                        SelectAll(projection);
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
                IModelProvider modelProvider,
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

                            BuildJoinExpressionRecursive(context, modelProvider, recursiveEnumerable, action);

                            BuildJoinOnExpression(context, modelProvider, relation);
                        });
                }
                else
                {
                    action?.Invoke();
                }

                static void BuildJoinOnExpression(
                    TranslationContext context,
                    IModelProvider modelProvider,
                    Relation relation)
                {
                    var targetPrimaryKeyColumn = modelProvider.Tables[relation.Target].Columns[nameof(IUniqueIdentified.PrimaryKey)];

                    context.Apply(new Expressions.BinaryExpression(
                        typeof(bool),
                        BinaryOperator.Equal,
                        new ColumnExpression(
                            relation.Target.Column(nameof(IUniqueIdentified.PrimaryKey)).Reflected,
                            targetPrimaryKeyColumn.Type,
                            ExtractParametersVisitor.ExtractParameter(context.Outer, relation.Target)),
                        new ColumnExpression(
                            relation.Property.Reflected,
                            targetPrimaryKeyColumn.Type,
                            ExtractParametersVisitor.ExtractParameter(context.Outer, relation.Source))));
                }
            }
        }*/

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
    }
}