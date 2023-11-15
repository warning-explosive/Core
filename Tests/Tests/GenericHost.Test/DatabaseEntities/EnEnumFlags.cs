namespace SpaceEngineers.Core.GenericHost.Test.DatabaseEntities
{
    using System;

    [Flags]
    internal enum EnEnumFlags
    {
        /// <summary>
        /// A
        /// </summary>
        A = 1 << 0,

        /// <summary>
        /// B
        /// </summary>
        B = 1 << 1,

        /// <summary>
        /// c
        /// </summary>
        C = 1 << 2
    }
}