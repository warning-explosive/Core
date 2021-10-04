namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Translation
{
    using System.Linq.Expressions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Orm.Linq;

    [Component(EnLifestyle.Scoped)]
    internal class QueryTranslator : IQueryTranslator
    {
        public IQuery Translate(Expression expression)
        {
            return new InMemoryQuery(expression);
        }
    }
}