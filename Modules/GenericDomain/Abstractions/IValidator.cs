namespace SpaceEngineers.Core.GenericDomain.Abstractions
{
    using AutoWiring.Api.Abstractions;

    /// <summary>
    /// Domain object validator
    /// </summary>
    /// <typeparam name="T">Domain-object type-argument</typeparam>
    public interface IValidator<T> : ICollectionResolvable<IValidator<T>>
        where T : IUniqueIdentified
    {
        /// <summary>
        /// Validate domain object
        /// </summary>
        /// <param name="item">Domain object instance</param>
        /// <returns>IValidationResult</returns>
        IValidationResult Validate(T item);
    }
}