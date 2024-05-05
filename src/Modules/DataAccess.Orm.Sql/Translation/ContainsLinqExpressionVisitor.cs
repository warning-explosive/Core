namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Expressions;
    using Linq;

    [Component(EnLifestyle.Singleton)]
    internal class ContainsLinqExpressionVisitor : ILinqExpressionVisitor,
                                                   ICollectionResolvable<ILinqExpressionVisitor>
    {
        private readonly TranslationExpressionVisitor _translationExpressionVisitor;

        public ContainsLinqExpressionVisitor(TranslationExpressionVisitor translationExpressionVisitor)
        {
            _translationExpressionVisitor = translationExpressionVisitor;
        }

        public bool TryVisit(
            ExpressionVisitor visitor,
            TranslationContext context,
            Expression expression)
        {
            if (expression is not System.Linq.Expressions.MethodCallExpression methodCallExpression)
            {
                return false;
            }

            var method = methodCallExpression.Method.GenericMethodDefinitionOrSelf();

            if (method == LinqMethods.QueryableContains())
            {
                if (methodCallExpression.Arguments[0] is not ConstantExpression constantExpression
                    || constantExpression.Value is not IQueryable subQuery)
                {
                    throw new InvalidOperationException("Unable to translate sub-query");
                }

                visitor.Visit(methodCallExpression.Arguments[1]);
                var left = context.SqlExpression;
                ISqlExpression right;

                using (context.WithinPathScope(constantExpression))
                {
                    var subQueryExpression = TranslateSubQuery(context, subQuery.Expression).Expression;
                    right = new ParenthesesExpression(subQueryExpression);
                }

                var binaryExpression = new Expressions.BinaryExpression(typeof(bool), BinaryOperator.Contains, left, right);
                context.Remember(binaryExpression);

                return true;
            }

            return false;
        }

        private SqlExpression TranslateSubQuery(
            TranslationContext context,
            Expression expression)
        {
            return _translationExpressionVisitor.Translate(context.Clone(), expression);
        }
    }
}