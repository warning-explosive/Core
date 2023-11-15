namespace SpaceEngineers.Core.GenericEndpoint.Web.Host
{
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Controllers;

    internal class CompositeControllerActivator : IControllerActivator
    {
        private readonly IReadOnlyDictionary<Type, IControllerActivator> _activators;

        public CompositeControllerActivator(
            IReadOnlyDictionary<Type, IControllerActivator> activators)
        {
            _activators = activators;
        }

        public object Create(ControllerContext context)
        {
            return SelectActivator(context).Create(context);
        }

        public void Release(ControllerContext context, object controller)
        {
            SelectActivator(context).Release(context, controller);
        }

        private IControllerActivator SelectActivator(ControllerContext context)
        {
            var controller = context.ActionDescriptor.ControllerTypeInfo.AsType();

            if (!_activators.TryGetValue(controller, out var activator))
            {
                throw new InvalidOperationException($"Unable to find activator for controller {controller}");
            }

            return activator;
        }
    }
}