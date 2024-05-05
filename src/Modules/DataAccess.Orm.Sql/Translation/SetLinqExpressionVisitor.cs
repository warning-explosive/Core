namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Expressions;
    using Linq;

    [Component(EnLifestyle.Singleton)]
    internal class SetLinqExpressionVisitor : ILinqExpressionVisitor,
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

            if (method == LinqMethods.RepositoryUpdateSet()
                || method == LinqMethods.RepositoryChainedUpdateSet())
            {
                visitor.Visit(methodCallExpression.Arguments[0]);
                var updateExpression = (UpdateExpression)context.SqlExpression;
                visitor.Visit(methodCallExpression.Arguments[1]);
                var assignment = (Expressions.BinaryExpression)context.SqlExpression;
                ((ICollection<Expressions.BinaryExpression>)updateExpression.Assignments).Add(assignment);
                context.Remember(updateExpression);

                return true;
            }

            return false;
        }
    }
}