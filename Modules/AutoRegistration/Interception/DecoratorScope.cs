namespace SpaceEngineers.Core.AutoRegistration.Interception
{
    using System;

    internal class DecoratorScope
    {
        private readonly Type _serviceType;

        internal DecoratorScope(Type serviceType)
        {
            _serviceType = serviceType;
        }

        private DecoratorScope? Outer { get; set; }

        internal DecoratorScope Open(Type serviceType)
        {
            for (var current = this; current != null; current = current.Outer)
            {
                if (current._serviceType == serviceType)
                {
                    throw new InvalidOperationException($"Cyclic reference {serviceType}");
                }
            }

            return new DecoratorScope(serviceType)
                   {
                       Outer = this
                   };
        }

        internal bool AlreadyOpened(Type serviceType)
        {
            for (var current = this; current != null; current = current.Outer)
            {
                if (current._serviceType == serviceType)
                {
                    return true;
                }
            }

            return false;
        }
    }
}