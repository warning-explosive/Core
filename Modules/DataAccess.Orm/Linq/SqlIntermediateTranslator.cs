namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;
    using Basics.Primitives;
    using Contract.Abstractions;
    using GenericDomain.Abstractions;
    using ValueObjects;
    using BinaryExpression = System.Linq.Expressions.BinaryExpression;
    using ConditionalExpression = System.Linq.Expressions.ConditionalExpression;
    using ConstantExpression = System.Linq.Expressions.ConstantExpression;
    using MethodCallExpression = System.Linq.Expressions.MethodCallExpression;
    using NewExpression = System.Linq.Expressions.NewExpression;
    using ParameterExpression = System.Linq.Expressions.ParameterExpression;

    [Component(EnLifestyle.Scoped)]
    internal class SqlIntermediateTranslator : IIntermediateTranslator
    {
        private const string CouldNotFindMethodFormat = "Could not find {0} method";
        private const string UnableToTranslateFormat = "Unable to translate {0}";

        private static readonly MethodInfo Select = QueryableSelect();
        private static readonly MethodInfo Where = QueryableWhere();

        private readonly IEnumerable<ISqlExpressionProvider> _sqlFunctionProviders;

        public SqlIntermediateTranslator(IEnumerable<ISqlExpressionProvider> sqlFunctionProviders)
        {
            _sqlFunctionProviders = sqlFunctionProviders;
        }

        public IIntermediateExpression Translate(Expression expression)
        {
            var visitor = new IntermediateExpressionVisitor(this, _sqlFunctionProviders);

            _ = visitor.Visit(expression);

            return visitor
                .Expression
                .EnsureNotNull(string.Format(UnableToTranslateFormat, nameof(expression)));
        }

        private static MethodInfo All(Type itemType)
        {
            return new MethodFinder(typeof(IReadRepository<>).MakeGenericType(itemType),
                    nameof(IReadRepository<IEntity>.All),
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod)
                .FindMethod()
                .EnsureNotNull(string.Format(CouldNotFindMethodFormat, "SpaceEngineers.Core.DataAccess.Contract.Abstractions.IReadRepository<>.All()"));
        }

        private static MethodInfo QueryableSelect()
        {
            return new MethodFinder(typeof(Queryable),
                    nameof(System.Linq.Queryable.Select),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object), typeof(object) },
                    ArgumentTypes = new[] { typeof(IQueryable<object>), typeof(Expression<Func<object, object>>) }
                }
                .FindMethod()
                .EnsureNotNull(string.Format(CouldNotFindMethodFormat, "System.Linq.Queryable.Select()"));
        }

        private static MethodInfo QueryableWhere()
        {
            return new MethodFinder(typeof(Queryable),
                    nameof(System.Linq.Queryable.Where),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object) },
                    ArgumentTypes = new[] { typeof(IQueryable<object>), typeof(Expression<Func<object, bool>>) }
                }
                .FindMethod()
                .EnsureNotNull(string.Format(CouldNotFindMethodFormat, "System.Linq.Queryable.Where()"));
        }

        private sealed class IntermediateExpressionVisitor : ExpressionVisitor
        {
            private readonly SqlIntermediateTranslator _translator;
            private readonly IEnumerable<ISqlExpressionProvider> _sqlFunctionProviders;

            private readonly Stack<IIntermediateExpression> _stack;

            public IntermediateExpressionVisitor(
                SqlIntermediateTranslator translator,
                IEnumerable<ISqlExpressionProvider> sqlFunctionProviders)
            {
                _translator = translator;
                _sqlFunctionProviders = sqlFunctionProviders;

                _stack = new Stack<IIntermediateExpression>();
            }

            internal IIntermediateExpression? Expression { get; private set; }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                var method = node.Method.IsGenericMethod
                    ? node.Method.GetGenericMethodDefinition()
                    : node.Method;

                var itemType = node.Type.UnwrapTypeParameter(typeof(IQueryable<>));

                if (itemType.IsClass
                    && typeof(IEntity).IsAssignableFrom(itemType)
                    && method == All(itemType))
                {
                    if (_stack.TryPeek(out var outer)
                        && outer is ProjectionExpression)
                    {
                        return WithScopeOpening(new QuerySourceExpression(itemType),
                            () => base.VisitMethodCall(node));
                    }

                    return WithScopeOpening(new ProjectionExpression(itemType, new QuerySourceExpression(itemType), Enumerable.Empty<IIntermediateExpression>()),
                        () => base.VisitMethodCall(node));
                }

                if (method == Select)
                {
                    return WithScopeOpening(new ProjectionExpression(itemType),
                        () => base.VisitMethodCall(node));
                }

                if (method == Where)
                {
                    return WithoutScopeDuplication(() => new FilterExpression(itemType),
                        () => base.VisitMethodCall(node));
                }

                if (TryRecognizeSqlExpression(node.Method, out var recognized))
                {
                    return WithScopeOpening(recognized, () => base.VisitMethodCall(node));
                }

                throw new NotSupportedException(string.Format(UnableToTranslateFormat, $"method: {node.Method}"));
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                IIntermediateExpression expression = TryRecognizeSqlExpression(node.Member, out var recognized)
                    ? recognized
                    : new SimpleBindingExpression(node.Type, node.Member.Name);

                return WithScopeOpening(expression, () => base.VisitMember(node));
            }

            protected override Expression VisitNew(NewExpression node)
            {
                return WithoutScopeOpening(() => new Orm.ValueObjects.NewExpression(node.Type),
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

                        return node;
                    });
            }

            protected override Expression VisitConditional(ConditionalExpression node)
            {
                return WithScopeOpening(new Orm.ValueObjects.ConditionalExpression(node.Type),
                    () => base.VisitConditional(node));
            }

            protected override Expression VisitBinary(BinaryExpression node)
            {
                return WithScopeOpening(new Orm.ValueObjects.BinaryExpression(node.Type, node.NodeType.ToString()),
                    () => base.VisitBinary(node));
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return WithScopeOpening(new Orm.ValueObjects.ParameterExpression(node.Type, node.Name),
                    () => base.VisitParameter(node));
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (node.Type.IsSubclassOfOpenGeneric(typeof(IReadRepository<>)))
                {
                    return base.VisitConstant(node);
                }

                return WithScopeOpening(new Orm.ValueObjects.ConstantExpression(node.Type, node.Value),
                    () => base.VisitConstant(node));
            }

            private static void Apply(IIntermediateExpression current, IIntermediateExpression outer)
            {
                ExecutionExtensions
                    .Try(CallApply)
                    .Catch<Exception>()
                    .Invoke(ex => throw new InvalidOperationException($"Could not apply {current.GetType().Name} for {outer.GetType().Name}", ex));

                void CallApply()
                {
                    outer.CallMethod(nameof(Apply)).WithArgument(current).Invoke();
                }
            }

            private void Apply<T>(T expression)
                where T : class, IIntermediateExpression
            {
                if (_stack.TryPeek(out var outer))
                {
                    Apply(expression, outer);
                }
            }

            private Expression WithScopeOpening<T>(T expression, Func<Expression> producer)
                where T : class, IIntermediateExpression
            {
                using (Disposable.Create(_stack, Push, Pop))
                {
                    return producer();
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

            private Expression WithoutScopeDuplication<T>(Func<T> intermediateExpressionProducer, Func<Expression> producer)
                where T : class, IIntermediateExpression
            {
                if (_stack.TryPeek(out var outer)
                    && outer is T)
                {
                    return producer();
                }

                return WithScopeOpening(intermediateExpressionProducer(), producer);
            }

            private Expression WithoutScopeOpening<T>(Func<T> intermediateExpressionProducer, Func<Expression> producer)
                where T : class, IIntermediateExpression
            {
                var produced = producer();

                if (_stack.TryPeek(out var outer))
                {
                    Apply(intermediateExpressionProducer(), outer);
                }

                return produced;
            }

            private bool TryRecognizeSqlExpression(MemberInfo memberInfo, [NotNullWhen(true)] out IIntermediateExpression? expression)
            {
                expression = _sqlFunctionProviders
                    .Select(provider =>
                    {
                        var success = provider.TryRecognize(memberInfo, out var info);
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
        }
    }
}