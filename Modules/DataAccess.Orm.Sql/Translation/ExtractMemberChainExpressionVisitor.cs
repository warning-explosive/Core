namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Persisting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    internal class ExtractMemberChainExpressionVisitor : ExpressionVisitor
    {
        private readonly List<PropertyInfo> _chain;

        public ExtractMemberChainExpressionVisitor()
        {
            _chain = new List<PropertyInfo>();
        }

        public PropertyInfo[] Chain
        {
            get => _chain.Any()
                ? _chain.ToArray()
                : throw new InvalidOperationException("Could not find any member chain");
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            _chain.Add((PropertyInfo)node.Member);

            return base.VisitMember(node);
        }
    }
}