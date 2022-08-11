namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using Api.Model;
    using Basics;
    using Model;

    internal class ExtractRelationsExpressionVisitor : ExpressionVisitor
    {
        private readonly IModelProvider _modelProvider;
        private readonly HashSet<Relation> _relations;

        public ExtractRelationsExpressionVisitor(IModelProvider modelProvider)
        {
            _modelProvider = modelProvider;
            _relations = new HashSet<Relation>();
        }

        public IReadOnlyCollection<Relation> Extract(Expression node)
        {
            _ = Visit(node);

            return _relations;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Member is PropertyInfo property
                && node.Type.IsSubclassOfOpenGeneric(typeof(IUniqueIdentified<>)))
            {
                _relations.Add(new Relation(node.Expression.Type, node.Type, new ColumnProperty(property, property), _modelProvider));
            }

            return base.VisitMember(node);
        }
    }
}