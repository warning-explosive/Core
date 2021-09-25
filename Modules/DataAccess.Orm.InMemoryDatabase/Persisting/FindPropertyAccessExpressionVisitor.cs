namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Persisting
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    internal class FindPropertyAccessExpressionVisitor : ExpressionVisitor
    {
        private PropertyInfo? _propertyInfo;

        public PropertyInfo PropertyInfo
        {
            get => _propertyInfo ?? throw new InvalidOperationException("Could not find property to change");
            private set => _propertyInfo = value;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Member.MemberType == MemberTypes.Property)
            {
                PropertyInfo = (PropertyInfo)node.Member;
            }

            return base.VisitMember(node);
        }
    }
}