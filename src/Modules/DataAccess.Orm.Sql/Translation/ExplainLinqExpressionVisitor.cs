namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Linq.Expressions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Expressions;
    using Linq;

    [Component(EnLifestyle.Singleton)]
    internal class ExplainLinqExpressionVisitor : ILinqExpressionVisitor,
                                                  ICollectionResolvable<ILinqExpressionVisitor>
    {
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

            if (method == LinqMethods.Explain())
            {
                visitor.Visit(methodCallExpression.Arguments[0]);
                var source = context.SqlExpression;
                var analyze = (bool)((ConstantExpression)methodCallExpression.Arguments[1]).Value;
                var explainExpression = new ExplainExpression(source, analyze);
                context.Remember(explainExpression);

                return true;
            }

            return false;
        }
    }
}