namespace SpaceEngineers.Core.AutoWiringApi.Abstractions
{
    /// <summary>
    /// Represents initializable object
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