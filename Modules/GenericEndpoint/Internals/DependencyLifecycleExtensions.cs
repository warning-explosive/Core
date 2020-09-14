﻿namespace SpaceEngineers.Core.GenericEndpoint.Internals
{
    using System;
    using AutoWiringApi.Enumerations;
    using NServiceBus;

    internal static class DependencyLifecycleExtensions
    {
        internal static EnLifestyle MapLifestyle(this DependencyLifecycle lifecycle)
        {
            return lifecycle switch
                   {
                       DependencyLifecycle.InstancePerCall => EnLifestyle.Transient,
                       DependencyLifecycle.SingleInstance => EnLifestyle.Singleton,
                       DependencyLifecycle.InstancePerUnitOfWork => EnLifestyle.Scoped,
                       _ => throw new NotSupportedException(lifecycle.ToString())
                   };
        }
    }
}