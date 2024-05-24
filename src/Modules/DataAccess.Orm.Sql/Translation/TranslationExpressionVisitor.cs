namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Expressions;
    using Model;
    using BinaryExpression = System.Linq.Expressions.BinaryExpression;
    using ConditionalExpression = System.Linq.Expressions.ConditionalExpression;
    using MethodCallExpression = System.Linq.Expressions.MethodCallExpression;
    using NewExpression = System.Linq.Expressions.NewExpression;
    using ParameterExpression = System.Linq.Expressions.ParameterExpression;
    using UnaryExpression = System.Linq.Expressions.UnaryExpression;

    /// <summary>
    /// TranslationExpressionVisitor
    /// </summary>
    [Component(EnLifestyle.Singleton)]
    internal class TranslationExpressionVisitor : ExpressionVisitor, // TODO: visibility (make public)
                                                IResolvable<TranslationExpressionVisitor>
    {
        private readonly IModelProvider _modelProvider;
        private readonly ILinqExpressionPreprocessorComposite _preprocessor;
        private readonly IEnumerable<ILinqExpressionVisitor> _linqExpressionVisitors;

        /// <summary> .cctor </summary>
        /// <param name="modelProvider">IModelProvider</param>
        /// <param name="preprocessor">ILinqExpressionPreprocessorComposite</param>
        /// <param name="linqExpressionVisitors">ILinqExpressionVisitor</param>
        public TranslationExpressionVisitor(
            IModelProvider modelProvider,
            ILinqExpressionPreprocessorComposite preprocessor,
            IEnumerable<ILinqExpressionVisitor> linqExpressionVisitors)
        {
            _modelProvider = modelProvider;
            _preprocessor = preprocessor;
            _linqExpressionVisitors = linqExpressionVisitors;
        }

        /// <summary>
        /// Translates System.Linq.Expressions.Expression to SqlExpression
        /// </summary>
        /// <param name="context">TranslationContext</param>
        /// <param name="expression">Expression</param>
        /// <returns>SqlExpression</returns>
        public SqlExpression Translate(
            TranslationContext context,
            Expression expression)
        {
            var visitor = new InternalTranslationExpressionVisitor(_modelProvider, this, context, expression);

            return visitor.Translate();
        }

        private class InternalTranslationExpressionVisitor : ExpressionVisitor
        {
            private readonly IModelProvider _modelProvider;
            private readonly TranslationExpressionVisitor _visitor;
            private readonly TranslationContext _context;
            private readonly Expression _expression;

            internal InternalTranslationExpressionVisitor(
                IModelProvider modelProvider,
                TranslationExpressionVisitor visitor,
                TranslationContext context,
                Expression expression)
            {
                _modelProvider = modelProvider;
                _visitor = visitor;
                _context = context;
                _expression = expression;
            }

            public sealed override Expression Visit(Expression node)
            {
                using (_context.WithinPathScope(node))
                {
                    base.Visit(node);

                    return node;
                }
            }

            internal SqlExpression Translate()
            {
                Visit(_visitor._preprocessor.Visit(_expression));

                return new SqlExpression(
                    _context.SqlExpression ?? throw new InvalidOperationException("Sql expression wasn't built"),
                    _context.BuildCommandParametersExtractor(_visitor._preprocessor));
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (TryVisitLinqExpression(node))
                {
                    return node;
                }

                throw new NotSupportedException($"method: {node.Method}");
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (TryVisitLinqExpression(node))
                {
                    return node;
                }

                Visit(node.Expression);
                var source = _context.SqlExpression;

                if (node.Member is not PropertyInfo property)
                {
                    throw new NotSupportedException($"member {node.Member} is not supported");
                }

                ISqlExpression expression = source switch
                {
                    // TODO: test long relations chain
                    ColumnExpression columnExpression => new ColumnsChainExpression(node.Type, new[] { columnExpression.Member, property }, columnExpression.Source),
                    ColumnsChainExpression columnsChainExpression => new ColumnsChainExpression(node.Type, columnsChainExpression.Members.Concat(new[] { property }).ToArray(), columnsChainExpression.Source),
                    _ => new ColumnExpression(node.Type, property, source)
                };

                _context.Remember(expression);

                return node;
            }

            protected override Expression VisitNew(NewExpression node)
            {
                var parameters = new List<ISqlExpression>(node.Arguments.Count);

                foreach (var (memberInfo, argument) in node.Members.Zip(node.Arguments, (memberInfo, argument) => (memberInfo, argument)))
                {
                    Visit(argument);

                    if (argument is MemberExpression memberExpression
                        && memberExpression.Member.MemberType == MemberTypes.Property
                        && memberExpression.Member.Name.Equals(memberInfo.Name, StringComparison.OrdinalIgnoreCase)
                        && _context.SqlExpression is not ColumnsChainExpression)
                    {
                        parameters.Add(_context.SqlExpression);
                    }
                    else
                    {
                        var expression = _context.SqlExpression is ColumnExpression or ColumnsChainExpression
                            ? _context.SqlExpression
                            : new ParenthesesExpression(_context.SqlExpression);
                        var renameExpression = new RenameExpression(argument.Type, new[] { memberInfo }, expression);
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
                var memberExpression = _context
                    .Path
                    .Skip(1)
                    .FirstOrDefault() as MemberExpression;

                Type type;

                if (node.Type.IsSubclassOfOpenGeneric(typeof(IUniqueIdentified<>))
                    && _modelProvider
                        .Columns(node.Type)
                        .TryGetValue(memberExpression.Member.Name, out var columnInfo)
                    && columnInfo.IsRelation)
                {
                    type = memberExpression.Type;
                }
                else
                {
                    type = node.Type;
                }

                var parameterExpression = _context
                        .ParameterExpressions
                        .SingleOrDefault(it => it.Type == type)
                    ?? throw new InvalidOperationException($"Unable to find parameter for type {type.FullName}");

                if (ExtractUpdateQueryRootExpressionVisitor.IsUpdateQuery(_expression)
                    || ExtractDeleteQueryRootExpressionVisitor.IsDeleteQuery(_expression))
                {
                    parameterExpression.SkipInSql = true;
                }

                _context.Remember(parameterExpression);

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

            private bool TryVisitLinqExpression(Expression expression)
            {
                return _visitor
                    ._linqExpressionVisitors
                    .Where(visitor => visitor.TryVisit(this, _context, expression))
                    .InformativeSingleOrDefault(Amb, expression) != null;

                static string Amb(Expression expression, IEnumerable<ILinqExpressionVisitor> infos)
                {
                    return $"More than one translations are suitable for expression: {expression}";
                }
            }
        }
    }
}