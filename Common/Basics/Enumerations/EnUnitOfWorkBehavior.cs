namespace SpaceEngineers.Core.Basics.Enumerations
{
    /// <summary>
    /// EnUnitOfWorkBehavior
    /// </summary>
    public enum EnUnitOfWorkBehavior
    {
        /// <summary>
        /// Regular behavior
        /// After successful transaction opening tries to execute producer and finish the unit of work gracefully
        /// </summary>
        Regular,

        /// <summary>
        /// Skip producer behavior
        /// After successful transaction opening skips producer execution and tries finish the unit of work gracefully
        /// </summary>
        SkipProducer,

        /// <summary>
        /// Do not run behavior
        /// The unit of work is considered as non started, producer execution and graceful finish will be skipped
        /// </summary>
        DoNotRun
    }
}