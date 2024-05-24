namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using Basics;
    using Model;

    internal class RelationsExpressionVisitor : ExpressionVisitor
    {
        private readonly IModelProvider _modelProvider;
        private readonly ICollection<Relation> _relations;

        private RelationsExpressionVisitor(
            IModelProvider modelProvider,
            ICollection<Relation> relations)
        {
            _modelProvider = modelProvider;
            _relations = relations;
        }

        public static IReadOnlyCollection<Relation> ExtractRelations(
            IModelProvider modelProvider,
            Expression expression)
        {
            var relations = new HashSet<Relation>();
            var visitor = new RelationsExpressionVisitor(modelProvider, relations);
            _ = visitor.Visit(expression);
            return relations;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Member is PropertyInfo propertyInfo
                && propertyInfo.PropertyType.IsSubclassOfOpenGeneric(typeof(IUniqueIdentified<>))
                && _modelProvider.Columns(propertyInfo.ReflectedType!).TryGetValue(propertyInfo.Name, out var columnInfo)
                && columnInfo.Relation != null)
            {
                _relations.Add(columnInfo.Relation);
            }

            return base.VisitMember(node);
        }
    }
}