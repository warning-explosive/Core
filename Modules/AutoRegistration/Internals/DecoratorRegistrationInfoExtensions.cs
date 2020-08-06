namespace SpaceEngineers.Core.AutoRegistration.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoWiringApi.Attributes;

    internal static class DecoratorRegistrationInfoExtensions
    {
        internal static IEnumerable<ServiceRegistrationInfo> GetDecoratorInfo(this IEnumerable<Type> decorators, Type decoratorType)
        {
            return GetGenericDecoratorInfo(decorators, decoratorType).Select(pair => pair.Info);
        }

        internal static IEnumerable<ServiceRegistrationInfo> GetConditionalDecoratorInfo(this IEnumerable<Type> decorators, Type decoratorType)
        {
            return GetGenericDecoratorInfo(decorators, decoratorType)
               .Select(pair =>
                       {
                           pair.Info.Attribute = pair.Decorator.GetGenericArguments()[1];
                           return pair.Info;
                       });
        }

        private static IEnumerable<(Type Decorator, ServiceRegistrationInfo Info)> GetGenericDecoratorInfo(IEnumerable<Type> decorators, Type decoratorType)
        {
            return decorators
                  .Where(RegistrationExtensions.ForAutoRegistration)
                  .Select(t => new
                               {
                                   ComponentType = t,
                                   Decorator = ExtractDecorator(decoratorType, t),
                                   Lifestyle = t.Lifestyle()
                               })
                  .Select(t => (t.Decorator, new ServiceRegistrationInfo(t.Decorator.GetGenericArguments()[0], t.ComponentType, t.Lifestyle)));
        }

        private static Type ExtractDecorator(Type decoratorType, Type t)
        {
            return t.GetInterfaces()
                    .Where(i => i.IsGenericType)
                    .Single(i => i.GetGenericTypeDefinition() == decoratorType);
        }
    }
}