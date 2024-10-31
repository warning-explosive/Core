namespace SpaceEngineers.Core.Basics.UnitOfWork;

public enum EnUnitOfWorkBehavior
{
    /// <summary>
    /// Regular behavior
    /// After successful transaction opening tries to execute producer and finish the unit of work gracefully
    /// </summary>
    Regular = 0,

    /// <summary>
    /// Skip producer behavior
    /// After successful transaction opening skips producer execution and tries finish the unit of work gracefully
    /// </summary>
    SkipProducer = 1,

    /// <summary>
    /// Do not run behavior
    /// The unit of work is considered as non started, producer execution and graceful finish will be skipped
    /// </summary>
    DoNotRun = 2
}