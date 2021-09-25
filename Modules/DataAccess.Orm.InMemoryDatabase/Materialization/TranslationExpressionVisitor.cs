namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Materialization
{
    using System.Linq;
    using System.Linq.Expressions;
    using Api.Model;
    using Api.Transaction;
    using Basics;
    using Orm.Linq;

    internal class TranslationExpressionVisitor : ExpressionVisitor
    {
        private readonly IAdvancedDatabaseTransaction _transaction;

        public TranslationExpressionVisitor(IAdvancedDatabaseTransaction transaction)
        {
            _transaction = transaction;
        }

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
                return Expression.Constant(_transaction.All(itemType));
            }

            return base.VisitMethodCall(node);
        }
    }
}