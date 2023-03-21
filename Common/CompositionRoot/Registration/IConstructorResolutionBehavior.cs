namespace SpaceEngineers.Core.CompositionRoot.Registration
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    /// <summary>
    /// IConstructorResolutionBehavior
    /// </summary>
    public interface IConstructorResolutionBehavior
    {
        /// <summary>
        /// Try get constructor
        /// </summary>
        /// <param name="implementation">implementation</param>
        /// <param name="cctor">Constructor</param>
        /// <returns>Constructor was found or not</returns>
        bool TryGetConstructor(Type implementation, [NotNullWhen(true)] out ConstructorInfo? cctor);
    }
}