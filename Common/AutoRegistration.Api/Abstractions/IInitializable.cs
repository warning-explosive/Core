namespace SpaceEngineers.Core.AutoRegistration.Api.Abstractions
{
    /// <summary>
    /// Represents initializable object
    /// Initializable components can't be injected into another components as dependency, but can be resolved manually
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