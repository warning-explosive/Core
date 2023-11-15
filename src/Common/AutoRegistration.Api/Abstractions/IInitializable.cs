namespace SpaceEngineers.Core.AutoRegistration.Api.Abstractions
{
    /// <summary>
    /// Represents resolvable object that should be initialized manually with runtime data
    /// </summary>
    /// <typeparam name="TRunTimeInput">TRunTimeInput type-argument</typeparam>
    public interface IInitializable<in TRunTimeInput>
    {
        /// <summary>
        /// Initialize object
        /// </summary>
        /// <param name="runTimeInput">Run-time input</param>
        void Initialize(TRunTimeInput runTimeInput);
    }
}