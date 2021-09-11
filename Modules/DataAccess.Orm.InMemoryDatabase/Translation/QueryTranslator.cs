namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Translation
{
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Linq;

    [Component(EnLifestyle.Scoped)]
    internal class QueryTranslator : IQueryTranslator
    {
        public Task<IQuery> Translate(Expression expression, CancellationToken token)
        {
            return Task.FromResult(new InMemoryQuery(expression) as IQuery);
        }
    }
}