﻿namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using Api.Model;
    using Basics;
    using Model;

    internal class ExtractRelationsExpressionVisitor : ExpressionVisitor
    {
        private readonly HashSet<Relation> _relations;

        public ExtractRelationsExpressionVisitor()
        {
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
                && node.Type.IsSubclassOfOpenGeneric(typeof(IDatabaseEntity<>)))
            {
                _relations.Add(new Relation(node.Expression.Type, node.Type, property));
            }

            return base.VisitMember(node);
        }
    }
}