namespace SpaceEngineers.Core.GenericDomain.Api.Abstractions
{
    using System;
    using Basics;

    /// <summary>
    /// IEnumerationObject
    /// </summary>
    public interface IEnumerationObject : IDomainObject,
                                          IEquatable<IEnumerationObject>,
                                          ISafelyEquatable<IEnumerationObject>
    {
        /// <summary>
        /// Identifier
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; }
    }
}