namespace SpaceEngineers.Core.Basics
{
    /// <summary>
    /// LogicalExtensions
    /// </summary>
    public static class LogicalExtensions
    {
        /// <summary> Bit </summary>
        /// <param name="condition">Condition</param>
        /// <returns>0 or 1</returns>
        public static int Bit(this bool condition)
        {
            return condition ? 1 : 0;
        }
    }
}