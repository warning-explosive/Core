namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System.Linq.Expressions;
    using Abstractions;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    // TODO: Remove
    [Component(EnLifestyle.Scoped)]
    internal class EmptyQueryVisitor : IQueryVisitor,
                                       ICollectionResolvable<IQueryVisitor>
    {
        public Expression Apply(Expression expression)
        {
            return expression;
        }
    }
}