namespace SpaceEngineers.Core.CrossCuttingConcerns.ObjectBuilder
{
    using System;
    using System.ComponentModel;

    internal class ObjectBuilderTypeDescriptorContext : ITypeDescriptorContext
    {
        public IContainer Container => throw Throw();

        public object Instance => throw Throw();

        public PropertyDescriptor PropertyDescriptor => throw Throw();

        public object GetService(Type serviceType)
        {
            throw Throw();
        }

        public void OnComponentChanged()
        {
            throw Throw();
        }

        public bool OnComponentChanging()
        {
            throw Throw();
        }

        private static Exception Throw()
        {
            throw new InvalidOperationException($"{typeof(ObjectBuilderTypeDescriptorContext)} should be only used as marker context");
        }
    }
}