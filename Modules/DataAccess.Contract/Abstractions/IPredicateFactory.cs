namespace SpaceEngineers.Core.DataAccess.Contract.Abstractions
{
    using System;
    using System.Linq.Expressions;
    using AutoWiring.Api.Abstractions;
    using GenericDomain.Abstractions;

    /// <summary>
    /// IPredicateFactory
    /// </summary>
    /// <typeparam name="TAggregate">TAggregate type-argument</typeparam>
    /// <typeparam name="TSpecification">TSpecification type-argument</typeparam>
    public interface IPredicateFactory<TAggregate, TSpecification> : IResolvable
        where TAggregate : class, IAggregate
        where TSpecification : IReadRepositorySpecification
    {
        /// <summary>
        /// Builds query predicate
        /// </summary>
        /// <param name="specification">Query specification</param>
        /// <returns>Built predicate</returns>
        Expression<Func<TAggregate, bool>> Build(TSpecification specification);
    }
}