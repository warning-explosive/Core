namespace SpaceEngineers.Core.CrossCuttingConcerns.ObjectBuilder
{
    /// <summary>
    /// IObjectTransformer
    /// </summary>
    /// <typeparam name="TSource">TSource type-argument</typeparam>
    /// <typeparam name="TTarget">TTarget type-argument</typeparam>
    public interface IObjectTransformer<in TSource, out TTarget>
    {
        /// <summary>
        /// Transforms the source value to target type value
        /// </summary>
        /// <param name="value">Source value</param>
        /// <returns>Result of conversion</returns>
        TTarget Transform(TSource value);
    }
}