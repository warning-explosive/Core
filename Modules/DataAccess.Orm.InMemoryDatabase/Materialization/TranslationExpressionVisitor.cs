namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Materialization
{
    using System.Linq;
    using System.Linq.Expressions;
    using Api.DatabaseEntity;
    using Basics;
    using Database;
    using Linq;

    internal class TranslationExpressionVisitor : ExpressionVisitor
    {
        private readonly IInMemoryDatabase _database;

        public TranslationExpressionVisitor(IInMemoryDatabase database)
        {
            _database = database;
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
                return Expression.Constant(_database.All(itemType));
            }

            return base.VisitMethodCall(node);
        }
    }
}