namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Linq;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseJsonObjectValueLinqExpressionVisitor : ILinqExpressionVisitor,
                                                                  ICollectionResolvable<ILinqExpressionVisitor>
    {
        public bool TryVisit(
            ExpressionVisitor visitor,
            TranslationContext context,
            Expression expression)
        {
            if (expression is MemberExpression memberExpression
                && memberExpression.Member is PropertyInfo propertyInfo
                && typeof(DatabaseJsonObject).IsAssignableFrom(propertyInfo.DeclaringType)
                && (propertyInfo.Name.Equals(nameof(DatabaseJsonObject.Value), StringComparison.OrdinalIgnoreCase)
                    || propertyInfo.Name.Equals(nameof(DatabaseJsonObject<object>.TypedValue), StringComparison.OrdinalIgnoreCase)))
            {
                visitor.Visit(memberExpression.Expression);

                return true;
            }

            return false;
        }
    }
}