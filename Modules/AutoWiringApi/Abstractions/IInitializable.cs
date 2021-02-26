namespace SpaceEngineers.Core.AutoWiringApi.Abstractions
{
    /// <summary>
    /// Represents initializable object
    /// Initializable components can't be injected into another components as dependency, but can be resolved manually
    /// </summary>
    /// <typeparam name="TData">TData type-argument</typeparam>
    public interface IInitializable<in TData>
    {
        /// <summary>
        /// Initialize object
        /// </summary>
        /// <param name="inputData">Input</param>
        void Initialize(TData inputData);
    }
}