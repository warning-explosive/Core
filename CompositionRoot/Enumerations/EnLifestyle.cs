namespace SpaceEngineers.Core.CompositionRoot.Enumerations
{
    /// <summary>
    /// Service lifestyle
    /// </summary>
    public enum EnLifestyle
    {
        /// <summary>
        /// Transient lifestyle 
        /// </summary>
        Transient = 1,
        
        /// <summary>
        /// Singleton lifestyle 
        /// </summary>
        Singleton = 2,
        
        /// <summary>
        /// Scoped lifestyle 
        /// </summary>
        Scoped = 4,
    }
}