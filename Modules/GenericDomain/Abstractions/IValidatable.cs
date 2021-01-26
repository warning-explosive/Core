namespace SpaceEngineers.Core.GenericDomain.Abstractions
{
    using System.Collections.Generic;

    /// <summary>
    /// Validatable domain-object
    /// </summary>
    /// <typeparam name="T">Domain-object type-argument</typeparam>
    public interface IValidatable<T>
        where T : IUniqueIdentified
    {
        /// <summary>
        /// Validate domain object
        /// </summary>
        /// <param name="validators">Validators</param>
        /// <returns>IValidationResult</returns>
        IValidationResult Validate(IReadOnlyCollection<IValidator<T>> validators);
    }
}